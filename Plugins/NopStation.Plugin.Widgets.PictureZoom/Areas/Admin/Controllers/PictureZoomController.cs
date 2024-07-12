using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PictureZoom.Infrastructure.Cache;

namespace NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Controllers;

public partial class PictureZoomController : NopStationAdminController
{
    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly PictureZoomSettings _pictureZoomSetting;
    private readonly IPermissionService _permissionService;
    private readonly IStaticCacheManager _cacheManager;
    private readonly ISettingService _settingService;

    public PictureZoomController(IStoreContext storeContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        PictureZoomSettings pictureZoomSetting,
        IStaticCacheManager staticCacheManager,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _storeContext = storeContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _pictureZoomSetting = pictureZoomSetting;
        _permissionService = permissionService;
        _cacheManager = staticCacheManager;
        _settingService = settingService;
    }

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(PictureZoomPermissionProvider.ManagePictureZoom))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var pictureZoomSettings = await _settingService.LoadSettingAsync<PictureZoomSettings>(storeId);

        var model = pictureZoomSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return View(model);

        model.EnablePictureZoom_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.EnablePictureZoom, storeId);
        model.AdjustX_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.AdjustX, storeId);
        model.AdjustY_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.AdjustY, storeId);
        model.FullSizeImage_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.FullSizeImage, storeId);
        model.ImageSize_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.ImageSize, storeId);
        model.LensOpacity_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.LensOpacity, storeId);
        model.LtrPositionTypeId_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.LtrPositionTypeId, storeId);
        model.RtlPositionTypeId_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.RtlPositionTypeId, storeId);
        model.ShowTitle_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.ShowTitle, storeId);
        model.SmoothMove_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.SmoothMove, storeId);
        model.SoftFocus_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.SoftFocus, storeId);
        model.TintOpacity_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.TintOpacity, storeId);
        model.Tint_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.Tint, storeId);
        model.TitleOpacity_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.TitleOpacity, storeId);
        model.ZoomHeight_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.ZoomHeight, storeId);
        model.ZoomWidth_OverrideForStore = await _settingService.SettingExistsAsync(pictureZoomSettings, x => x.ZoomWidth, storeId);

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PictureZoomPermissionProvider.ManagePictureZoom))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var pictureZoomSettings = await _settingService.LoadSettingAsync<PictureZoomSettings>(storeId);
        pictureZoomSettings = model.ToSettings(pictureZoomSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.EnablePictureZoom, model.EnablePictureZoom_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.AdjustX, model.AdjustX_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.AdjustY, model.AdjustY_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.FullSizeImage, model.FullSizeImage_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.ImageSize, model.ImageSize_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.LensOpacity, model.LensOpacity_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.LtrPositionTypeId, model.LtrPositionTypeId_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.RtlPositionTypeId, model.RtlPositionTypeId_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.ShowTitle, model.ShowTitle_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.SmoothMove, model.SmoothMove_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.SoftFocus, model.SoftFocus_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.TintOpacity, model.TintOpacity_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.Tint, model.Tint_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.TitleOpacity, model.TitleOpacity_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.ZoomHeight, model.ZoomHeight_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(pictureZoomSettings, x => x.ZoomWidth, model.ZoomWidth_OverrideForStore, storeId, false);

        await _settingService.ClearCacheAsync();
        await _cacheManager.RemoveByPrefixAsync(ModelCacheEventConsumer.PrictureZoom_patern_key);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}
