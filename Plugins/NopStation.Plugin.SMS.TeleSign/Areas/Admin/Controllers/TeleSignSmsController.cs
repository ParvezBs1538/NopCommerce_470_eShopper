using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.SMS.TeleSign.Areas.Admin.Factories;
using NopStation.Plugin.SMS.TeleSign.Areas.Admin.Models;
using NopStation.Plugin.SMS.TeleSign.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;

namespace NopStation.Plugin.SMS.TeleSign.Areas.Admin.Controllers
{
    public class TeleSignSmsController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ITeleSignModelFactory _teleSignModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ISmsSender _smsSender;

        #endregion

        #region Ctor

        public TeleSignSmsController(IPermissionService permissionService,
            INotificationService notificationService,
            ITeleSignModelFactory teleSignModelFactory,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            ISmsSender smsSender)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _teleSignModelFactory = teleSignModelFactory;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _smsSender = smsSender;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(TeleSignSmsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _teleSignModelFactory.PrepareConfigurationModelAsync();
            return View(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(TeleSignSmsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var teleSignSettings = await _settingService.LoadSettingAsync<TeleSignSettings>(storeScope);
            teleSignSettings = model.ToSettings(teleSignSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.From, model.PhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.CheckPhoneNumberRegex, model.CheckPhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.CheckIntlDialCode, model.CheckIntlDialCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.IntlDialCode, model.IntlDialCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.ApiSecret, model.ApiSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(teleSignSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [EditAccess, HttpPost, ActionName("Configure")]
        [FormValueRequired("sendtestsms")]
        public virtual async Task<IActionResult> SendTestSms(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(TeleSignSmsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            try
            {
                if (string.IsNullOrWhiteSpace(model.SendTestSmsTo))
                    throw new NopException("Enter test phone number");

                var response = _smsSender.SendNotification(model.SendTestSmsTo, await _localizationService.GetResourceAsync("Admin.NopStation.TeleSignSms.Configuration.SendTestSms.Body"), "admin_test");

                if (response.Status.Code != 1)
                    _notificationService.ErrorNotification(response.Status.Description);
                else
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.TeleSignSms.Configuration.SendTestSms.Success"));
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
