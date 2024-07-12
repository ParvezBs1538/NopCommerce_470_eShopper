using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Controllers
{
    public class AffiliateStationController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAffiliateStationModelFactory _affiliateStationModelFactory;

        #endregion

        #region Ctor

        public AffiliateStationController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IAffiliateStationModelFactory affiliateStationModelFactory)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _affiliateStationModelFactory = affiliateStationModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _affiliateStationModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AffiliateStationSettings>(storeScope);
            settings = model.ToSettings(settings);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AffiliatePageOrderPageSize, model.AffiliatePageOrderPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CommissionAmount, model.CommissionAmount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CommissionPercentage, model.CommissionPercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseDefaultCommissionIfNotSetOnCatalog, model.UseDefaultCommissionIfNotSetOnCatalog_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UsePercentage, model.UsePercentage_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
