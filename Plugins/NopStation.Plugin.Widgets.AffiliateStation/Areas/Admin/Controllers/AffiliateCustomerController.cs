using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Extensions;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Controllers
{
    public class AffiliateCustomerController : NopStationAdminController
    {
        #region Fileds

        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly IAffiliateCustomerModelFactory _affiliateCustomerModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public AffiliateCustomerController(IAffiliateCustomerService affiliateCustomerService,
            IAffiliateCustomerModelFactory affiliateCustomerModelFactory,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _affiliateCustomerService = affiliateCustomerService;
            _affiliateCustomerModelFactory = affiliateCustomerModelFactory;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        #endregion

        #region Utilities



        #endregion

        #region Methods

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageAffiliateCustomer))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageAffiliateCustomer))
                return AccessDeniedView();

            var model = await _affiliateCustomerModelFactory.PrepareAffiliateCustomerSearchModelAsync();
            return View(model);
        }

        public async Task<IActionResult> GetList(AffiliateCustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageAffiliateCustomer))
                return AccessDeniedView();

            var model = await _affiliateCustomerModelFactory.PrepareAffiliateCustomerListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> SaveDetails(AffiliateCustomerModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageAffiliateCustomer))
                return AccessDeniedView();

            //try to get a affiliate customer with the specified id
            var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByAffiliateIdAsync(model.AffiliateId);
            if (affiliateCustomer == null)
                return Json(new { Result = false, Message = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.AffiliateCustomers.NotFound") });

            affiliateCustomer = model.ToEntity(affiliateCustomer);
            affiliateCustomer.UpdatedOnUtc = DateTime.UtcNow;

            await _affiliateCustomerService.UpdateAffiliateCustomerAsync(affiliateCustomer);

            return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.AffiliateCustomers.Updated") });
        }

        #endregion
    }
}
