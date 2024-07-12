using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public class WebApiNotificationModelFactory : IWebApiNotificationModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public WebApiNotificationModelFactory(ISettingService settingService,
            IStoreContext storeContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync(ConfigurationModel model)
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<WebApiNotificationSettings>(storeId);

            model = settings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeId;
            model.AvailableApplicationTypes = (await ApplicationType.Native.ToSelectListAsync()).ToList();

            if (storeId == 0)
                return model;

            model.GoogleConsoleApiAccessKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.GoogleConsoleApiAccessKey, storeId);
            model.ApplicationTypeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ApplicationTypeId, storeId);
            model.PushKitClientSecret_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PushKitClientSecret, storeId);
            model.PushKitClientId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PushKitClientId, storeId);
            model.PushKitAppId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PushKitAppId, storeId);

            return model;
        }

        #endregion
    }
}
