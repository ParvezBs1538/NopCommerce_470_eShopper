using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.Messente.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.SMS.Messente.Areas.Admin.Factories
{
    public class MessenteSmsModelFactory : IMessenteSmsModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MessenteSmsModelFactory(ISettingService settingService,
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
            var messenteSmsSettings = await _settingService.LoadSettingAsync<MessenteSmsSettings>(storeScope);

            var model = messenteSmsSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.EnablePlugin, storeScope);
                model.Username_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.Username, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.Password, storeScope);
                model.SenderName_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.SenderName, storeScope);
                model.CheckPhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.CheckPhoneNumberRegex, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.PhoneNumberRegex, storeScope);
                model.CheckIntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.CheckIntlDialCode, storeScope);
                model.IntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.IntlDialCode, storeScope);
                model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(messenteSmsSettings, x => x.EnableLog, storeScope);
            }

            return model;
        }

        #endregion
    }
}
