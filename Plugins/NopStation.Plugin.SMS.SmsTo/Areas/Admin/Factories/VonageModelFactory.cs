using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.SmsTo.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.SMS.SmsTo.Areas.Admin.Factories
{
    public class SmsToModelFactory : ISmsToModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public SmsToModelFactory(ISettingService settingService,
            IStoreContext storeContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion
        
        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var smsToSettings = await _settingService.LoadSettingAsync<SmsToSettings>(storeScope);

            var model = smsToSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.EnablePlugin, storeScope);
                model.SenderId_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.SenderId, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.ApiKey, storeScope);
                model.PhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.From, storeScope);
                model.CheckPhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.CheckPhoneNumberRegex, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.PhoneNumberRegex, storeScope);
                model.CheckIntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.CheckIntlDialCode, storeScope);
                model.IntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.IntlDialCode, storeScope);
                model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(smsToSettings, x => x.EnableLog, storeScope);
            }

            return model;
        }

        #endregion
    }
}
