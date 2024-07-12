using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.BulkSms.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.SMS.BulkSms.Areas.Admin.Factories
{
    public class BulkSmsModelFactory : IBulkSmsModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public BulkSmsModelFactory(ISettingService settingService,
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
            var bulkSmsSettings = await _settingService.LoadSettingAsync<BulkSmsSettings>(storeScope);

            var model = bulkSmsSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.EnablePlugin, storeScope);
                model.Username_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.Username, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.Password, storeScope);
                model.CheckPhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.CheckPhoneNumberRegex, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.PhoneNumberRegex, storeScope);
                model.CheckIntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.CheckIntlDialCode, storeScope);
                model.IntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.IntlDialCode, storeScope);
                model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(bulkSmsSettings, x => x.EnableLog, storeScope);
            }

            return model;
        }

        #endregion
    }
}
