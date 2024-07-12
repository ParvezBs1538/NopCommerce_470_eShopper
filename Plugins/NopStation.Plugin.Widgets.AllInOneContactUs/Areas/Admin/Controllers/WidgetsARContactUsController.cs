using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.AllInOneContactUs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Announcement;

namespace NopStation.Plugin.Widgets.AllInOneContactUs.Areas.Admin.Controllers
{
    public class WidgetsARContactUsController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public WidgetsARContactUsController(IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AllInOneContactUsPermissionProvider.ManageAllInOneContactUs))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var arContactUsSettings = await _settingService.LoadSettingAsync<ARContactUsSettings>(storeScope);

            var model = arContactUsSettings.ToSettingsModel<WidgetsARContactUsConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnableMeetingLink_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableMeetingLink, storeScope);
                model.MeetingLink_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.MeetingLink, storeScope);
                model.EnableTeams_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableTeams, storeScope);
                model.TeamsId_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.TeamsId, storeScope);
                model.EnableMessenger_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableMessenger, storeScope);
                model.MessengerId_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.MessengerId, storeScope);
                model.EnableSkype_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableSkype, storeScope);
                model.SkypeId_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.SkypeId, storeScope);
                model.EnableEmail_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableEmail, storeScope);
                model.EmailId_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EmailId, storeScope);
                model.EnableCall_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableCall, storeScope);
                model.PhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.PhoneNumber, storeScope);
                model.EnableTawkChat_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableTawkChat, storeScope);
                model.TawkChatSrc_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.TawkChatSrc, storeScope);
                model.EnableWhatsapp_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableWhatsapp, storeScope);
                model.WhatsappNumber_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.WhatsappNumber, storeScope);
                model.EnableDirectContactUs_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableDirectContactUs, storeScope);
                model.ContactUsPageUrl_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.ContactUsPageUrl, storeScope);

                model.EnableTelegram_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableTelegram, storeScope);
                model.TelegramName_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.TelegramName, storeScope);
                model.EnableViber_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.EnableViber, storeScope);
                model.ViberNumber_OverrideForStore = await _settingService.SettingExistsAsync(arContactUsSettings,
                    x => x.ViberNumber, storeScope);
            }
            return View("~/Plugins/NopStation.Plugin.Widgets.AllInOneContactUs/Areas/Admin/Views/WidgetsARContactUs/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(WidgetsARContactUsConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AllInOneContactUsPermissionProvider.ManageAllInOneContactUs))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var arContactUsSettings = await _settingService.LoadSettingAsync<ARContactUsSettings>(storeScope);

            arContactUsSettings = model.ToSettings(arContactUsSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableTeams, model.EnableTeams_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableMessenger, model.EnableMessenger_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.TeamsId, model.TeamsId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.MessengerId, model.MessengerId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableSkype, model.EnableSkype_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.SkypeId, model.SkypeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableEmail, model.EnableEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EmailId, model.EmailId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableCall, model.EnableCall_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.PhoneNumber, model.PhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableTawkChat, model.EnableTawkChat_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.TawkChatSrc, model.TawkChatSrc_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableWhatsapp, model.EnableWhatsapp_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.WhatsappNumber, model.WhatsappNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableDirectContactUs, model.EnableDirectContactUs_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.ContactUsPageUrl, model.ContactUsPageUrl_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableTelegram, model.EnableTelegram_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.TelegramName, model.TelegramName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableViber, model.EnableViber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.ViberNumber, model.ViberNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.EnableMeetingLink, model.EnableMeetingLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(arContactUsSettings, x => x.MeetingLink, model.MeetingLink_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
