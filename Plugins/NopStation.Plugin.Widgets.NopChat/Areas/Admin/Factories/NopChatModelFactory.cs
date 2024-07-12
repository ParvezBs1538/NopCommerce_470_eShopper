using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.NopChat.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.NopChat.Areas.Admin.Factories
{
    public class NopChatModelFactory : INopChatModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public NopChatModelFactory(ISettingService settingService,
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
            var nopChatSettings = await _settingService.LoadSettingAsync<NopChatSettings>(storeScope);

            var model = nopChatSettings.ToSettingsModel<ConfigurationModel>();

            if (storeScope > 0)
            {
                model.OpenOnMonday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnMonday, storeScope);
                model.OpenOnTuesday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnTuesday, storeScope);
                model.OpenOnWednesday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnWednesday, storeScope);
                model.OpenOnThursday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnThursday, storeScope);
                model.OpenOnFriday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnFriday, storeScope);
                model.OpenOnSaturday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnSaturday, storeScope);
                model.OpenOnSunday_OverrideForStore = await _settingService.SettingExistsAsync(nopChatSettings, x => x.OpenOnSunday, storeScope);
            }

            return model;
        }

        #endregion
    }
}
