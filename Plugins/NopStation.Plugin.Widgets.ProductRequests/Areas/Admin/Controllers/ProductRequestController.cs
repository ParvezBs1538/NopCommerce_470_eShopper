using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Widgets.ProductRequests.Services;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Factories;
using Nop.Services.Security;
using Nop.Core;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Controllers
{
    public partial class ProductRequestController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IProductRequestModelFactory _productRequestModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductRequestService _productRequestService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ProductRequestController(IStoreContext storeContext,
            IProductRequestModelFactory productRequestModelFactory,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProductRequestService productRequestService,
            ISettingService settingService)
        {
            _storeContext = storeContext;
            _productRequestModelFactory = productRequestModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _productRequestService = productRequestService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        #region Configure

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return AccessDeniedView();

            var model = await _productRequestModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productRequestSettings = await _settingService.LoadSettingAsync<ProductRequestSettings>(storeScope);
            productRequestSettings = model.ToSettings(productRequestSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.IncludeInFooter, model.IncludeInFooter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.IncludeInTopMenu, model.IncludeInTopMenu_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.FooterElementSelector, model.FooterElementSelector_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.AllowedCustomerRolesIds, model.AllowedCustomerRolesIds_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.DescriptionRequired, model.DescriptionRequired_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.LinkRequired, model.LinkRequired_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(productRequestSettings, x => x.MinimumProductRequestCreateInterval, model.MinimumProductRequestCreateInterval_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region List

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return AccessDeniedView();

            var model = await _productRequestModelFactory.PrepareProductRequestSearchModelAsync(new ProductRequestSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ProductRequestSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return await AccessDeniedDataTablesJson();

            var model = await _productRequestModelFactory.PrepareProductRequestListModelAsync(searchModel);
            return Json(model);
        }

        #endregion

        #region Update/delete

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return AccessDeniedView();

            var productRequest = await _productRequestService.GetProductRequestByIdAsync(id);
            if (productRequest == null || productRequest.Deleted)
                return RedirectToAction("List");

            var model = await _productRequestModelFactory.PrepareProductRequestModelAsync(null, productRequest);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Edit(ProductRequestModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return AccessDeniedView();

            var productRequest = await _productRequestService.GetProductRequestByIdAsync(model.Id);
            if (productRequest == null || productRequest.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productRequest = model.ToEntity(productRequest);

                await _productRequestService.UpdateProductRequestAsync(productRequest);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.Updated"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = model.Id }) :
                    RedirectToAction("List");
            }

            model = await _productRequestModelFactory.PrepareProductRequestModelAsync(model, productRequest);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async virtual Task<IActionResult> Delete(ProductRequestModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
                return AccessDeniedView();

            var productRequest = await _productRequestService.GetProductRequestByIdAsync(model.Id);
            if (productRequest == null || productRequest.Deleted)
                return RedirectToAction("List");

            await _productRequestService.DeleteProductRequestAsync(productRequest);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}
