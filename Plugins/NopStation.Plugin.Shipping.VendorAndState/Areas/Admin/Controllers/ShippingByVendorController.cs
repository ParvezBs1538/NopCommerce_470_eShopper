using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using NopStation.Plugin.Shipping.VendorAndState.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Controllers
{
    public class ShippingByVendorController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IVendorShippingFactory _vendorShippingFactory;
        private readonly ISettingService _settingService;
        private readonly IVendorShippingService _vendorShippingService;
        private readonly IVendorStateProvinceShippingFactory _vendorStateProvinceShippingFactory;
        private readonly IVendorStateProvinceShippingService _vendorStateProvinceShippingService;

        #endregion

        #region Ctor

        public ShippingByVendorController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IVendorShippingFactory vendorShippingFactory,
            ISettingService settingService,
            IVendorShippingService vendorShippingService,
            IVendorStateProvinceShippingFactory vendorStateProvinceShippingFactory,
            IVendorStateProvinceShippingService vendorStateProvinceShippingService)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _vendorShippingFactory = vendorShippingFactory;
            _settingService = settingService;
            _vendorShippingService = vendorShippingService;
            _vendorStateProvinceShippingFactory = vendorStateProvinceShippingFactory;
            _vendorStateProvinceShippingService = vendorStateProvinceShippingService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shippingByVendorSettings = await _settingService.LoadSettingAsync<VendorAndStateSettings>(storeId);

            var model = shippingByVendorSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.AmountX_OverrideForStore = await _settingService.SettingExistsAsync(shippingByVendorSettings, x => x.AmountX, storeId);
            model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(shippingByVendorSettings, x => x.EnablePlugin, storeId);
            model.ShippingCharge_OverrideForStore = await _settingService.SettingExistsAsync(shippingByVendorSettings, x => x.ShippingCharge, storeId);
            model.EnableFreeShippingOverAmountX_OverrideForStore = await _settingService.SettingExistsAsync(shippingByVendorSettings, x => x.EnableFreeShippingOverAmountX, storeId);
            model.WithDiscounts_OverrideForStore = await _settingService.SettingExistsAsync(shippingByVendorSettings, x => x.WithDiscounts, storeId);
            model.TransitDays_OverrideForStore = await _settingService.SettingExistsAsync(shippingByVendorSettings, x => x.TransitDays, storeId);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shippingByVendorSettings = await _settingService.LoadSettingAsync<VendorAndStateSettings>(storeScope);

            shippingByVendorSettings = model.ToSettings(shippingByVendorSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(shippingByVendorSettings, x => x.AmountX, model.AmountX_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingByVendorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingByVendorSettings, x => x.ShippingCharge, model.ShippingCharge_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingByVendorSettings, x => x.EnableFreeShippingOverAmountX, model.EnableFreeShippingOverAmountX_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingByVendorSettings, x => x.WithDiscounts, model.WithDiscounts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingByVendorSettings, x => x.TransitDays, model.TransitDays_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> ShippingCharges()
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var searchModel = await _vendorShippingFactory.PrepareVendorShippingSearchModelAsync(new VendorShippingSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> ShippingCharges(VendorShippingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var model = await _vendorShippingFactory.PrepareVendorShippingListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> ShippingCharge(int vendorId, int shippingMethodId)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();
            
            var vendorShipping = await _vendorShippingService.GetVendorShippingByVendorIdAndShippingMethodIdAsync(vendorId, shippingMethodId);
            var model = await _vendorShippingFactory.PrepareVendorShippingModelAsync(vendorShipping != null ? null :
                new VendorShippingModel(), vendorShipping, true);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ShippingCharge(VendorShippingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var vendorShipping = await _vendorShippingService.GetVendorShippingByVendorIdAndShippingMethodIdAsync(model.VendorId, model.ShippingMethodId);
            if (vendorShipping == null)
            {
                vendorShipping = model.ToEntity<VendorShipping>();
                await _vendorShippingService.InsertVendorShippingAsync(vendorShipping);
            }
            else
            {
                vendorShipping = model.ToEntity(vendorShipping);
                await _vendorShippingService.UpdateVendorShippingAsync(vendorShipping);
            }

            return Json(new { Resuylt = true });
        }

        [HttpPost]
        public async Task<IActionResult> StateShippingCharges(VendorStateProvinceShippingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var model = await _vendorStateProvinceShippingFactory.PrepareVendorStateProvinceShippingListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> StateShippingChargeUpdate(VendorStateProvinceShippingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var token = model.ComplexId.Split(new[] { "___" }, StringSplitOptions.None);
            model.StateProvinceId = Convert.ToInt32(token[0]);
            model.VendorId = Convert.ToInt32(token[1]);
            model.ShippingMethodId = Convert.ToInt32(token[2]);

            var vendorStateProvinceShipping = await _vendorStateProvinceShippingService.GetVendorStateProvinceShippingByVendorIdAndShippingMethodIdAsync(
                model.VendorId, model.ShippingMethodId, model.StateProvinceId);

            if (vendorStateProvinceShipping == null)
            {
                vendorStateProvinceShipping = model.ToEntity<VendorStateProvinceShipping>();
                await _vendorStateProvinceShippingService.InsertVendorStateProvinceShippingAsync(vendorStateProvinceShipping);
            }
            else
            {
                model.Id = vendorStateProvinceShipping.Id;
                vendorStateProvinceShipping = model.ToEntity(vendorStateProvinceShipping);
                await _vendorStateProvinceShippingService.UpdateVendorStateProvinceShippingAsync(vendorStateProvinceShipping);
            }

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> StateShippingChargeDelete(VendorStateProvinceShippingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            if (await _vendorStateProvinceShippingService.GetVendorStateProvinceShippingByIdAsync(model.Id) is VendorStateProvinceShipping vendorStateProvinceShipping)
                await _vendorStateProvinceShippingService.DeleteVendorStateProvinceShippingAsync(vendorStateProvinceShipping);

            return new NullJsonResult();
        }

        #endregion
    }
}
