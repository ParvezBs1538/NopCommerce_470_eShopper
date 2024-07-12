using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Controllers
{
    public class WalletActivityHistoryController : NopStationAdminController
    {
        #region Fields

        private readonly IActivityHistoryModelFactory _activityHistoryModelFactory;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public WalletActivityHistoryController(IActivityHistoryModelFactory activityHistoryModelFactory,
            IPermissionService permissionService)
        {
            _activityHistoryModelFactory = activityHistoryModelFactory;
            _permissionService = permissionService;
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

            var searchModel = await _activityHistoryModelFactory.PrepareActivityHistorySearchModelAsync(new ActivityHistorySearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ActivityHistorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return await AccessDeniedDataTablesJson();

            var model = await _activityHistoryModelFactory.PrepareActivityHistoryListModelAsync(searchModel);
            return Json(model);
        }

        #endregion
    }
}