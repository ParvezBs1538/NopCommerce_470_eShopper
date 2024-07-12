using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Controllers
{
    public partial class ProductRibbonController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Ctor

        public ProductRibbonController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICurrencyService currencyService,
            CurrencySettings currencySettings)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ProductRibbonPermissionProvider.ManageProductRibbon))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ribbonSettings = await _settingService.LoadSettingAsync<ProductRibbonSettings>(storeId);

            var model = ribbonSettings.ToSettingsModel<ConfigurationModel>();
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, false);
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(model.AvailablePaymentStatuses, false);
            await _baseAdminModelFactory.PrepareShippingStatusesAsync(model.AvailableShippingStatuses, false);

            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            model.ActiveStoreScopeConfiguration = storeId;
            model.CurrencyCode = currency?.CurrencyCode;

            if (storeId <= 0)
                return View(model);

            model.ProductDetailsPageWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.ProductDetailsPageWidgetZone, storeId);
            model.ProductOverviewBoxWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.ProductOverviewBoxWidgetZone, storeId);
            model.EnableBestSellerRibbon_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.EnableBestSellerRibbon, storeId);
            model.EnableNewRibbon_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.EnableNewRibbon, storeId);
            model.EnableDiscountRibbon_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.EnableDiscountRibbon, storeId);
            model.SoldInDays_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.SoldInDays, storeId);
            model.BestSellStoreWise_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.BestSellStoreWise, storeId);
            model.BestSellPaymentStatusIds_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.BestSellPaymentStatusIds, storeId);
            model.BestSellOrderStatusIds_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.BestSellOrderStatusIds, storeId);
            model.BestSellShippingStatusIds_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.BestSellShippingStatusIds, storeId);
            model.MinimumAmountSold_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.MinimumAmountSold, storeId);
            model.MinimumQuantitySold_OverrideForStore = await _settingService.SettingExistsAsync(ribbonSettings, x => x.MinimumQuantitySold, storeId);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductRibbonPermissionProvider.ManageProductRibbon))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ribbonSettings = await _settingService.LoadSettingAsync<ProductRibbonSettings>(storeScope);
            ribbonSettings = model.ToSettings(ribbonSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.ProductDetailsPageWidgetZone, model.ProductDetailsPageWidgetZone_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.ProductOverviewBoxWidgetZone, model.ProductOverviewBoxWidgetZone_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.EnableBestSellerRibbon, model.EnableBestSellerRibbon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.EnableDiscountRibbon, model.EnableDiscountRibbon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.EnableNewRibbon, model.EnableNewRibbon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.SoldInDays, model.SoldInDays_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.BestSellStoreWise, model.BestSellStoreWise_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.BestSellPaymentStatusIds, model.BestSellPaymentStatusIds_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.BestSellOrderStatusIds, model.BestSellOrderStatusIds_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.BestSellShippingStatusIds, model.BestSellShippingStatusIds_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.MinimumAmountSold, model.MinimumAmountSold_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ribbonSettings, x => x.MinimumQuantitySold, model.MinimumQuantitySold_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductRibbon.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
