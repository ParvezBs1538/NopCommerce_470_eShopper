using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Controllers
{
    public class WalletController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWalletModelFactory _walletModelFactory;
        private readonly IWalletService _walletService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IActivityHistoryService _activityHistoryService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public WalletController(ILocalizationService localizationService,
            IWalletModelFactory walletModelFactory,
            IWalletService walletService,
            IPermissionService permissionService,
            ICustomerService customerService,
            IActivityHistoryService activityHistoryService,
            IWorkContext workContext)
        {
            _localizationService = localizationService;
            _walletModelFactory = walletModelFactory;
            _walletService = walletService;
            _permissionService = permissionService;
            _customerService = customerService;
            _activityHistoryService = activityHistoryService;
            _workContext = workContext;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var searchModel = await _walletModelFactory.PrepareWalletSearchModelAsync(new WalletSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(WalletSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return await AccessDeniedDataTablesJson();

            var model = await _walletModelFactory.PrepareWalletListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> CreateOrUpdate(WalletModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var customer = await _customerService.GetCustomerByIdAsync(model.WalletCustomerId)
                ?? throw new ArgumentNullException($"No customer found with the specific id {model.WalletCustomerId}");

            if (ModelState.IsValid)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (await _walletService.GetWalletByCustomerIdAsync(model.WalletCustomerId) is Wallet wallet)
                {
                    var oldCreditLimt = wallet.CreditLimit;
                    wallet = model.ToEntity(wallet);
                    var creditDiff = oldCreditLimt - wallet.CreditLimit;
                    wallet.AvailableCredit -= creditDiff;
                    await _walletService.UpdateWalletAsync(wallet);

                    var prevActivity = (await _activityHistoryService.GetWalletActivityHistoryAsync(wallet)).FirstOrDefault();
                    var activityNote = "";
                    if (creditDiff == 0)
                        activityNote = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Wallet.UpdateActivity.NoDiff"),
                            currentCustomer.Email);
                    else if (creditDiff < 0)
                        activityNote = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Wallet.UpdateActivity.Increased"),
                            -creditDiff, currentCustomer.Email);
                    else
                        activityNote = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Wallet.UpdateActivity.Decreased"),
                            creditDiff, currentCustomer.Email);

                    var activity = new ActivityHistory()
                    {
                        ActivityType = ActivityType.AdminModify,
                        CreatedByCustomerId = currentCustomer.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        WalletCustomerId = model.WalletCustomerId,
                        Note = activityNote,
                        CurrentTotalCreditUsed = prevActivity?.CurrentTotalCreditUsed ?? 0,
                        PreviousTotalCreditUsed = prevActivity?.PreviousTotalCreditUsed ?? 0
                    };
                    await _activityHistoryService.InsertActivityHistoryAsync(activity);

                    return Json(new
                    {
                        message = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Wallet.Updated"),
                        success = true
                    });
                }
                else
                {
                    var newWallet = model.ToEntity<Wallet>();
                    newWallet.AvailableCredit = model.CreditLimit;
                    await _walletService.InsertWalletAsync(newWallet);

                    var activity = new ActivityHistory()
                    {
                        ActivityType = ActivityType.AdminCreate,
                        CreatedByCustomerId = currentCustomer.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        WalletCustomerId = model.WalletCustomerId,
                        Note = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Wallet.CreateActivity"),
                            newWallet.AvailableCredit, currentCustomer.Email),
                        CurrentTotalCreditUsed = 0,
                        PreviousTotalCreditUsed = 0
                    };
                    await _activityHistoryService.InsertActivityHistoryAsync(activity);

                    return Json(new
                    {
                        message = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Wallet.Created"),
                        success = true
                    });
                }
            }

            return Json(new
            {
                success = false,
                errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList()
            });
        }

        #endregion
    }
}