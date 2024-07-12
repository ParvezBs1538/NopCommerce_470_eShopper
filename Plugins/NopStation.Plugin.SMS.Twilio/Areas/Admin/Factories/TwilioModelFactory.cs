using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.Twilio.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.SMS.Twilio.Areas.Admin.Factories
{
    public class TwilioModelFactory : ITwilioModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public TwilioModelFactory(ISettingService settingService,
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
            var twilioSettings = await _settingService.LoadSettingAsync<TwilioSettings>(storeScope);

            var model = twilioSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.EnablePlugin, storeScope);
                model.AccountSID_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.AccountSID, storeScope);
                model.AuthToken_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.AuthToken, storeScope);
                model.SubAccount_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.SubAccount, storeScope);
                model.PhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.PhoneNumber, storeScope);
                model.CheckPhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.CheckPhoneNumberRegex, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.PhoneNumberRegex, storeScope);
                model.CheckIntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.CheckIntlDialCode, storeScope);
                model.IntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.IntlDialCode, storeScope);
                model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(twilioSettings, x => x.EnableLog, storeScope);
            }

            return model;
        }

        #endregion
    }
}
