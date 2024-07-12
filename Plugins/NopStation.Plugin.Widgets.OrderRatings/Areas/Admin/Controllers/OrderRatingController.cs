using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Controllers
{
    public partial class OrderRatingController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public OrderRatingController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(OrderRatingPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var orderRatingSettings = await _settingService.LoadSettingAsync<OrderRatingSettings>(storeId);

            var model = orderRatingSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.OpenOrderRatingPopupInHomepage_OverrideForStore = await _settingService.SettingExistsAsync(orderRatingSettings, x => x.OpenOrderRatingPopupInHomepage, storeId);
            model.OrderDetailsPageWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(orderRatingSettings, x => x.OrderDetailsPageWidgetZone, storeId);
            model.ShowOrderRatedDateInDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(orderRatingSettings, x => x.ShowOrderRatedDateInDetailsPage, storeId);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(OrderRatingPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var orderRatingSettings = await _settingService.LoadSettingAsync<OrderRatingSettings>(storeScope);

            orderRatingSettings = model.ToSettings(orderRatingSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(orderRatingSettings, x => x.OpenOrderRatingPopupInHomepage, model.OpenOrderRatingPopupInHomepage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderRatingSettings, x => x.OrderDetailsPageWidgetZone, model.OrderDetailsPageWidgetZone_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderRatingSettings, x => x.ShowOrderRatedDateInDetailsPage, model.ShowOrderRatedDateInDetailsPage_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
