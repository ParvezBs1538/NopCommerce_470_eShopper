using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.TeleSign.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.SMS.TeleSign.Areas.Admin.Factories
{
    public class TeleSignModelFactory : ITeleSignModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public TeleSignModelFactory(ISettingService settingService,
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
            var teleSignSettings = await _settingService.LoadSettingAsync<TeleSignSettings>(storeScope);

            var model = teleSignSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.EnablePlugin, storeScope);
                model.ApiSecret_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.ApiSecret, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.ApiKey, storeScope);
                model.PhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.From, storeScope);
                model.CheckPhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.CheckPhoneNumberRegex, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.PhoneNumberRegex, storeScope);
                model.CheckIntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.CheckIntlDialCode, storeScope);
                model.IntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.IntlDialCode, storeScope);
                model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(teleSignSettings, x => x.EnableLog, storeScope);
            }

            return model;
        }

        #endregion
    }
}
