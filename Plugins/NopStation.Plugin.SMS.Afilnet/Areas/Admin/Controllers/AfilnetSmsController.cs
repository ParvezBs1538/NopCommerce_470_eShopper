using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.SMS.Afilnet.Areas.Admin.Factories;
using NopStation.Plugin.SMS.Afilnet.Areas.Admin.Models;
using NopStation.Plugin.SMS.Afilnet.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;

namespace NopStation.Plugin.SMS.Afilnet.Areas.Admin.Controllers
{
    public class AfilnetSmsController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IAfilnetModelFactory _afilnetModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ISmsSender _smsSender;

        #endregion

        #region Ctor

        public AfilnetSmsController(IPermissionService permissionService,
            INotificationService notificationService,
            IAfilnetModelFactory afilnetModelFactory,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            ISmsSender smsSender)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _afilnetModelFactory = afilnetModelFactory;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _smsSender = smsSender;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AfilnetPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _afilnetModelFactory.PrepareConfigurationModelAsync();
            return View(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AfilnetPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var afilnetSettings = await _settingService.LoadSettingAsync<AfilnetSettings>(storeScope);
            afilnetSettings = model.ToSettings(afilnetSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.From, model.From_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.CheckPhoneNumberRegex, model.CheckPhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.CheckIntlDialCode, model.CheckIntlDialCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.IntlDialCode, model.IntlDialCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afilnetSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [EditAccess, HttpPost, ActionName("Configure")]
        [FormValueRequired("sendtestsms")]
        public virtual async Task<IActionResult> SendTestSms(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AfilnetPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            try
            {
                if (string.IsNullOrWhiteSpace(model.SendTestSmsTo))
                    throw new NopException("Enter test phone number");

                await _smsSender.SendNotificationAsync(model.SendTestSmsTo, await _localizationService.GetResourceAsync("Admin.NopStation.Afilnet.Configuration.SendTestSms.Body"));
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Afilnet.Configuration.SendTestSms.Success"));
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
