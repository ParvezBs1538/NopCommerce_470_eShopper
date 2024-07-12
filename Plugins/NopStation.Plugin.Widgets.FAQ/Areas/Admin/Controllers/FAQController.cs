using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Controllers
{
    public class FAQController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public FAQController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var faqSettings = await _settingService.LoadSettingAsync<FAQSettings>(storeScope);

            var model = new ConfigurationModel
            {
                EnablePlugin = faqSettings.EnablePlugin,
                FooterElementSelector = faqSettings.FooterElementSelector,
                IncludeInFooter = faqSettings.IncludeInFooter,
                IncludeInTopMenu = faqSettings.IncludeInTopMenu,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(faqSettings, x => x.EnablePlugin, storeScope);
                model.FooterElementSelector_OverrideForStore = await _settingService.SettingExistsAsync(faqSettings, x => x.FooterElementSelector, storeScope);
                model.IncludeInFooter_OverrideForStore = await _settingService.SettingExistsAsync(faqSettings, x => x.IncludeInFooter, storeScope);
                model.IncludeInTopMenu_OverrideForStore = await _settingService.SettingExistsAsync(faqSettings, x => x.FooterElementSelector, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var faqSettings = await _settingService.LoadSettingAsync<FAQSettings>(storeScope);

            faqSettings.EnablePlugin = model.EnablePlugin;
            faqSettings.FooterElementSelector = model.FooterElementSelector;
            faqSettings.IncludeInFooter = model.IncludeInFooter;
            faqSettings.IncludeInTopMenu = model.IncludeInTopMenu;

            await _settingService.SaveSettingOverridablePerStoreAsync(faqSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(faqSettings, x => x.FooterElementSelector, model.FooterElementSelector_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(faqSettings, x => x.IncludeInFooter, model.IncludeInFooter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(faqSettings, x => x.IncludeInTopMenu, model.IncludeInTopMenu_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
