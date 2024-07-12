using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public class AffiliateStationModelFactory : IAffiliateStationModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public AffiliateStationModelFactory(ISettingService settingService,
            IStoreContext storeContext,
            ICurrencyService currencyService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affiliateStationSettings = await _settingService.LoadSettingAsync<AffiliateStationSettings>(storeScope);
            var currencySettings = await _settingService.LoadSettingAsync<CurrencySettings>(storeScope);

            var model = affiliateStationSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;
            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

            if (storeScope <= 0)
                return model;

            model.AffiliatePageOrderPageSize_OverrideForStore = await _settingService.SettingExistsAsync(affiliateStationSettings, x => x.AffiliatePageOrderPageSize, storeScope);
            model.CommissionAmount_OverrideForStore = await _settingService.SettingExistsAsync(affiliateStationSettings, x => x.CommissionAmount, storeScope);
            model.CommissionPercentage_OverrideForStore = await _settingService.SettingExistsAsync(affiliateStationSettings, x => x.CommissionPercentage, storeScope);
            model.UseDefaultCommissionIfNotSetOnCatalog_OverrideForStore = await _settingService.SettingExistsAsync(affiliateStationSettings, x => x.UseDefaultCommissionIfNotSetOnCatalog, storeScope);
            model.UsePercentage_OverrideForStore = await _settingService.SettingExistsAsync(affiliateStationSettings, x => x.UsePercentage, storeScope);

            return model;
        }

        #endregion
    }
}
