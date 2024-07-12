using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.SmartProductRating.Models;

namespace NopStation.Plugin.Widgets.SmartProductRating.Controllers
{
    public class SmartProductRatingController : NopStationAdminController
    {
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;

        public SmartProductRatingController(ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext)
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(SmartProductRatingPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ratingSettings = await _settingService.LoadSettingAsync<SmartProductRatingSettings>(storeScope);

            var model = new ConfigurationModel();

            model.EnablePlugin = ratingSettings.EnablePlugin;
            model.NumberOfReviewsInProductDetailsPage = ratingSettings.NumberOfReviewsInProductDetailsPage;
            model.ProductDetailsPageWidgetZone = ratingSettings.ProductDetailsPageWidgetZone;
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(ratingSettings, x => x.EnablePlugin, storeScope);
                model.NumberOfReviewsInProductDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(ratingSettings, x => x.NumberOfReviewsInProductDetailsPage, storeScope);
                model.ProductDetailsPageWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(ratingSettings, x => x.ProductDetailsPageWidgetZone, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.SmartProductRating/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SmartProductRatingPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ratingSettings = await _settingService.LoadSettingAsync<SmartProductRatingSettings>(storeScope);

            ratingSettings.EnablePlugin = model.EnablePlugin;
            ratingSettings.NumberOfReviewsInProductDetailsPage = model.NumberOfReviewsInProductDetailsPage;
            ratingSettings.ProductDetailsPageWidgetZone = model.ProductDetailsPageWidgetZone;

            await _settingService.SaveSettingOverridablePerStoreAsync(ratingSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ratingSettings, x => x.NumberOfReviewsInProductDetailsPage, model.NumberOfReviewsInProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ratingSettings, x => x.ProductDetailsPageWidgetZone, model.ProductDetailsPageWidgetZone_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
