using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Controllers
{
    public class WalletInvoicePaymentController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IInvoicePaymentModelFactory _invoicePaymentModelFactory;
        private readonly IInvoicePaymentService _invoicePaymentService;
        private readonly IPermissionService _permissionService;
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IActivityHistoryService _activityHistoryService;

        #endregion

        #region Ctor

        public WalletInvoicePaymentController(ILocalizationService localizationService,
            INotificationService notificationService,
            IInvoicePaymentModelFactory invoicePaymentModelFactory,
            IInvoicePaymentService invoicePaymentService,
            IPermissionService permissionService,
            IWalletService walletService,
            IWorkContext workContext,
            IDateTimeHelper dateTimeHelper,
            IActivityHistoryService activityHistoryService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _invoicePaymentModelFactory = invoicePaymentModelFactory;
            _invoicePaymentService = invoicePaymentService;
            _permissionService = permissionService;
            _walletService = walletService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _activityHistoryService = activityHistoryService;
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

            var searchModel = await _invoicePaymentModelFactory.PrepareInvoicePaymentSearchModelAsync(new InvoicePaymentSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(InvoicePaymentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return await AccessDeniedDataTablesJson();

            var model = await _invoicePaymentModelFactory.PrepareInvoicePaymentListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create(int customerWalletId, bool fromCustomerPage)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var wallet = await _walletService.GetWalletByCustomerIdAsync(customerWalletId);
            if (wallet == null)
                return RedirectToAction("List");

            var model = await _invoicePaymentModelFactory.PrepareInvoicePaymentModelAsync(new InvoicePaymentModel(), null, wallet);
            model.FromCustomerPage = fromCustomerPage;

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(InvoicePaymentModel model, bool continueEditing, bool fromCustomerPage)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var wallet = await _walletService.GetWalletByCustomerIdAsync(model.WalletCustomerId);
            if (wallet == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var invoicePayment = model.ToEntity<InvoicePayment>();
                invoicePayment.PaymentDateUtc = _dateTimeHelper.ConvertToUtcTime(model.PaymentDate);
                invoicePayment.CreatedByCustomerId = currentCustomer.Id;
                invoicePayment.UpdatedByCustomerId = currentCustomer.Id;

                await _invoicePaymentService.InsertInvoicePaymentAsync(invoicePayment);

                wallet.AvailableCredit += invoicePayment.Amount;
                await _walletService.UpdateWalletAsync(wallet);

                var prevActivity = (await _activityHistoryService.GetWalletActivityHistoryAsync(wallet)).FirstOrDefault();
                var activity = new ActivityHistory()
                {
                    ActivityType = ActivityType.InvoicePayment,
                    CreatedByCustomerId = currentCustomer.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    WalletCustomerId = model.WalletCustomerId,
                    Note = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.CreateActivity"),
                        invoicePayment.Amount, invoicePayment.Id, currentCustomer.Email),
                    CurrentTotalCreditUsed = prevActivity?.CurrentTotalCreditUsed ?? 0,
                    PreviousTotalCreditUsed = prevActivity?.PreviousTotalCreditUsed ?? 0
                };
                await _activityHistoryService.InsertActivityHistoryAsync(activity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.Created"));

                if (continueEditing)
                    return RedirectToAction("Edit", new { id = invoicePayment.Id, fromCustomerPage = fromCustomerPage });

                if (model.FromCustomerPage)
                    return RedirectToAction("Edit", "Customer", new { id = invoicePayment.WalletCustomerId });

                return RedirectToAction("List");
            }

            model = await _invoicePaymentModelFactory.PrepareInvoicePaymentModelAsync(model, null, wallet);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id, bool fromCustomerPage)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var invoicePayment = await _invoicePaymentService.GetInvoicePaymentByIdAsync(id);
            if (invoicePayment == null)
                return RedirectToAction("List");

            var wallet = await _walletService.GetWalletByCustomerIdAsync(invoicePayment.WalletCustomerId);
            if (wallet == null)
                return RedirectToAction("List");

            var model = await _invoicePaymentModelFactory.PrepareInvoicePaymentModelAsync(null, invoicePayment, wallet);
            model.FromCustomerPage = fromCustomerPage;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(InvoicePaymentModel model, bool continueEditing, bool fromCustomerPage)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var invoicePayment = await _invoicePaymentService.GetInvoicePaymentByIdAsync(model.Id);
            if (invoicePayment == null)
                return RedirectToAction("List");

            var wallet = await _walletService.GetWalletByCustomerIdAsync(invoicePayment.WalletCustomerId);
            if (wallet == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var oldAmount = invoicePayment.Amount;
                invoicePayment = model.ToEntity(invoicePayment);
                invoicePayment.PaymentDateUtc = _dateTimeHelper.ConvertToUtcTime(model.PaymentDate);
                invoicePayment.UpdatedByCustomerId = currentCustomer.Id;

                await _invoicePaymentService.UpdateInvoicePaymentAsync(invoicePayment);

                var amountDiff = oldAmount - invoicePayment.Amount;
                wallet.AvailableCredit -= amountDiff;
                await _walletService.UpdateWalletAsync(wallet);

                string activityNote;
                if (amountDiff == 0)
                    activityNote = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity.NoDiff"),
                        currentCustomer.Email);
                else if (amountDiff < 0)
                    activityNote = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity.Increased"),
                        -amountDiff, invoicePayment.Id, currentCustomer.Email);
                else
                    activityNote = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity.Decreased"),
                        amountDiff, invoicePayment.Id, currentCustomer.Email);

                var prevActivity = (await _activityHistoryService.GetWalletActivityHistoryAsync(wallet)).FirstOrDefault();
                var activity = new ActivityHistory()
                {
                    ActivityType = ActivityType.InvoiceAdjustment,
                    CreatedByCustomerId = currentCustomer.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    WalletCustomerId = model.WalletCustomerId,
                    Note = activityNote,
                    CurrentTotalCreditUsed = prevActivity?.CurrentTotalCreditUsed ?? 0,
                    PreviousTotalCreditUsed = prevActivity?.PreviousTotalCreditUsed ?? 0
                };
                await _activityHistoryService.InsertActivityHistoryAsync(activity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.Updated"));

                if (continueEditing)
                    return RedirectToAction("Edit", new { id = invoicePayment.Id, fromCustomerPage = fromCustomerPage });

                if (model.FromCustomerPage)
                    return RedirectToAction("Edit", "Customer", new { id = invoicePayment.WalletCustomerId });

                return RedirectToAction("List");
            }
            model = await _invoicePaymentModelFactory.PrepareInvoicePaymentModelAsync(model, invoicePayment, wallet);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var invoicePayment = await _invoicePaymentService.GetInvoicePaymentByIdAsync(id);
            if (invoicePayment == null)
                return RedirectToAction("List");

            var wallet = await _walletService.GetWalletByCustomerIdAsync(invoicePayment.WalletCustomerId);
            if (wallet == null)
                return RedirectToAction("List");

            //Not required when 'InvoicePayment' is 'ISoftDeletedEntity'
            var amount = invoicePayment.Amount;
            var invoicePaymentId = invoicePayment.Id;
            var walletCustomerId = invoicePayment.WalletCustomerId;

            await _invoicePaymentService.DeleteInvoicePaymentAsync(invoicePayment);

            wallet.AvailableCredit -= amount;
            await _walletService.UpdateWalletAsync(wallet);

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var prevActivity = (await _activityHistoryService.GetWalletActivityHistoryAsync(wallet)).FirstOrDefault();
            var activity = new ActivityHistory()
            {
                ActivityType = ActivityType.InvoicePayment,
                CreatedByCustomerId = currentCustomer.Id,
                CreatedOnUtc = DateTime.UtcNow,
                WalletCustomerId = wallet.WalletCustomerId,
                Note = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.DeleteActivity"),
                    amount, invoicePaymentId, currentCustomer.Email),
                CurrentTotalCreditUsed = prevActivity?.CurrentTotalCreditUsed ?? 0,
                PreviousTotalCreditUsed = prevActivity?.PreviousTotalCreditUsed ?? 0
            };
            await _activityHistoryService.InsertActivityHistoryAsync(activity);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.InvoicePayments.Deleted"));

            return RedirectToAction("Edit", "Customer", new { id = walletCustomerId });
        }

        #endregion
    }
}