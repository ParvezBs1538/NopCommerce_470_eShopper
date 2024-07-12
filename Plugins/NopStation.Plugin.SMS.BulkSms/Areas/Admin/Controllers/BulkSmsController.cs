using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.SMS.BulkSms.Areas.Admin.Factories;
using NopStation.Plugin.SMS.BulkSms.Areas.Admin.Models;
using NopStation.Plugin.SMS.BulkSms.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.BulkSms.Areas.Admin.Controllers
{
    public class BulkSmsController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IBulkSmsModelFactory _bulkSmsModelFactory; 
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ISmsSender _smsSender;

        #endregion

        #region Ctor

        public BulkSmsController(IPermissionService permissionService,
            INotificationService notificationService,
            IBulkSmsModelFactory bulkSmsModelFactory,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            ISmsSender smsSender)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _bulkSmsModelFactory = bulkSmsModelFactory;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _smsSender = smsSender;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(BulkSmsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _bulkSmsModelFactory.PrepareConfigurationModelAsync();
            return View(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(BulkSmsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var bulkSmsSettings = await _settingService.LoadSettingAsync<BulkSmsSettings>(storeScope);
            bulkSmsSettings = model.ToSettings(bulkSmsSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.CheckPhoneNumberRegex, model.CheckPhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.CheckIntlDialCode, model.CheckIntlDialCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.IntlDialCode, model.IntlDialCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bulkSmsSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [EditAccess, HttpPost, ActionName("Configure")]
        [FormValueRequired("sendtestsms")]
        public virtual async Task<IActionResult> SendTestSms(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(BulkSmsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            try
            {
                if (string.IsNullOrWhiteSpace(model.SendTestSmsTo))
                    throw new NopException("Enter test phone number");

                var smsParameter = new SmsParameter()
                {
                    To = model.SendTestSmsTo,
                    Body = await _localizationService.GetResourceAsync("Admin.NopStation.BulkSms.Configuration.SendTestSms.Body")
                };
                _smsSender.SendNotification(new List<SmsParameter> { smsParameter });
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.BulkSms.Configuration.SendTestSms.Success"));
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
