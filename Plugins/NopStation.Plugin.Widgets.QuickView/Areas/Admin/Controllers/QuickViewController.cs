using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.QuickView.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.QuickView.Areas.Admin.Controllers;

public partial class QuickViewController : NopStationAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;
    private readonly IPluginService _pluginService;

    #endregion

    #region Ctor

    public QuickViewController(IStoreContext storeContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IWorkContext workContext,
        IPluginService pluginService)
    {
        _storeContext = storeContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _workContext = workContext;
        _pluginService = pluginService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(QuickViewPermissionProvider.ManageQuickView))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var quickViewSettings = await _settingService.LoadSettingAsync<QuickViewSettings>(storeId);

        var model = quickViewSettings.ToSettingsModel<ConfigurationModel>();

        var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("NopStation.Plugin.Widgets.PictureZoom",
            LoadPluginsMode.InstalledOnly, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
        model.PictureZoomPluginInstalled = pluginDescriptor != null;

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return View(model);

        model.ShowAlsoPurchasedProducts_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowAlsoPurchasedProducts, storeId);
        model.ShowRelatedProducts_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowRelatedProducts, storeId);
        model.ShowAddToWishlistButton_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowAddToWishlistButton, storeId);
        model.ShowAvailability_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowAvailability, storeId);
        model.ShowCompareProductsButton_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowCompareProductsButton, storeId);
        model.ShowDeliveryInfo_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowDeliveryInfo, storeId);
        model.ShowFullDescription_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowFullDescription, storeId);
        model.ShowProductEmailAFriendButton_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowProductEmailAFriendButton, storeId);
        model.ShowProductManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowProductManufacturers, storeId);
        model.ShowProductReviewOverview_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowProductReviewOverview, storeId);
        model.ShowProductSpecifications_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowProductSpecifications, storeId);
        model.ShowProductTags_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowProductTags, storeId);
        model.ShowRelatedProducts_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowRelatedProducts, storeId);
        model.ShowShortDescription_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.ShowShortDescription, storeId);
        model.EnablePictureZoom_OverrideForStore = await _settingService.SettingExistsAsync(quickViewSettings, x => x.EnablePictureZoom, storeId);

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuickViewPermissionProvider.ManageQuickView))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var quickViewSettings = await _settingService.LoadSettingAsync<QuickViewSettings>(storeScope);

        quickViewSettings = model.ToSettings(quickViewSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowAlsoPurchasedProducts, model.ShowAlsoPurchasedProducts_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowRelatedProducts, model.ShowRelatedProducts_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowAddToWishlistButton, model.ShowAddToWishlistButton_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowAvailability, model.ShowAvailability_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowCompareProductsButton, model.ShowCompareProductsButton_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowDeliveryInfo, model.ShowDeliveryInfo_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowFullDescription, model.ShowFullDescription_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowProductEmailAFriendButton, model.ShowProductEmailAFriendButton_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowProductManufacturers, model.ShowProductManufacturers_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowProductReviewOverview, model.ShowProductReviewOverview_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowProductSpecifications, model.ShowProductSpecifications_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowProductTags, model.ShowProductTags_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowRelatedProducts, model.ShowRelatedProducts_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.ShowShortDescription, model.ShowShortDescription_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quickViewSettings, x => x.EnablePictureZoom, model.EnablePictureZoom_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.QuickView.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}
