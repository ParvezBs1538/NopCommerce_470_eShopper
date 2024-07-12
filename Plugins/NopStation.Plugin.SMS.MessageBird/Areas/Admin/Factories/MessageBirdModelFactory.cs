using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.MessageBird.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.SMS.MessageBird.Areas.Admin.Factories
{
    public class MessageBirdModelFactory : IMessageBirdModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MessageBirdModelFactory(ISettingService settingService,
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
            var messageBirdSettings = await _settingService.LoadSettingAsync<MessageBirdSettings>(storeScope);

            var model = messageBirdSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.EnablePlugin, storeScope);
                model.Originator_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.Originator, storeScope);
                model.AccessKey_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.AccessKey, storeScope);
                model.CheckPhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.CheckPhoneNumberRegex, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.PhoneNumberRegex, storeScope);
                model.CheckIntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.CheckIntlDialCode, storeScope);
                model.IntlDialCode_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.IntlDialCode, storeScope);
                model.RemoveFirstNDigitsWhenLocalNumber_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.RemoveFirstNDigitsWhenLocalNumber, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(messageBirdSettings, x => x.EnableLog, storeScope);
            }

            return model;
        }

        #endregion
    }
}
