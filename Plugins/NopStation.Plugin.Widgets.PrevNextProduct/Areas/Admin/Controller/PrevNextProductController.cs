using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PrevNextProduct.Domains;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Controllers;

public class PrevNextProductController : NopStationAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public PrevNextProductController(IPermissionService permissionService,
        IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    #endregion

    #region Configure

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(PrevNextProductPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var prevNextProductSettings = await _settingService.LoadSettingAsync<PrevNextProductSettings>(storeId);

        var model = prevNextProductSettings.ToSettingsModel<ConfigurationModel>();
        model.AvailableNavigationTypes = (await NavigationType.Category.ToSelectListAsync()).ToList();

        if (storeId > 0)
        {
            model.EnableLoop_OverrideForStore = await _settingService.SettingExistsAsync(prevNextProductSettings, x => x.EnableLoop, storeId);
            model.WidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(prevNextProductSettings, x => x.WidgetZone, storeId);
            model.NavigateBasedOnId_OverrideForStore = await _settingService.SettingExistsAsync(prevNextProductSettings, x => x.NavigateBasedOnId, storeId);
            model.ProductNameMaxLength_OverrideForStore = await _settingService.SettingExistsAsync(prevNextProductSettings, x => x.ProductNameMaxLength, storeId);
        }

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PrevNextProductPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var prevNextProductSettings = await _settingService.LoadSettingAsync<PrevNextProductSettings>(storeScope);
        prevNextProductSettings = model.ToSettings(prevNextProductSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(prevNextProductSettings, x => x.EnableLoop, model.EnableLoop_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(prevNextProductSettings, x => x.WidgetZone, model.WidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(prevNextProductSettings, x => x.NavigateBasedOnId, model.NavigateBasedOnId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(prevNextProductSettings, x => x.ProductNameMaxLength, model.ProductNameMaxLength_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}
