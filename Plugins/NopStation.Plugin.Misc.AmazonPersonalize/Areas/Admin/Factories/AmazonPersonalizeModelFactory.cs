using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Factories
{
    public class AmazonPersonalizeModelFactory : IAmazonPersonalizeModelFactory
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Ctor

        public AmazonPersonalizeModelFactory(IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
        }

        #endregion Ctor

        #region Utilities

        protected async Task PrepareAwsRegionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            
            var availableWidgetZones = AwsRegionHelper.GetAwsRegionsSelectList();
            foreach (var zone in availableWidgetZones)
            {
                items.Add(zone);
            }

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableWidgetZones = RecommendationHelper.GetCustomWidgetZoneSelectList();
            foreach (var zone in availableWidgetZones)
            {
                items.Add(zone);
            }

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        #endregion Utilities

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var amazonPersonalizeSettings = await _settingService.LoadSettingAsync<AmazonPersonalizeSettings>(storeScope);

            var model = amazonPersonalizeSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            await PrepareAwsRegionsAsync(model.AvailableAwsRegions, false);
            await PrepareCustomWidgetZonesAsync(model.AvailableWidgetZones, false);
            if (storeScope == 0)
                return model;

            #region Common
            model.EnableAmazonPersonalize_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableAmazonPersonalize, storeScope);
            model.AccessKey_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.AccessKey, storeScope);
            model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.SecretKey, storeScope);
            model.AwsRegionId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.AwsRegionId, storeScope);
            model.EventTrackerId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EventTrackerId, storeScope);
            model.DataSetGroupArn_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.DataSetGroupArn, storeScope);
            model.EnableLogging_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableLogging, storeScope);
            #endregion

            #region RecommendedForYou
            model.EnableRecommendedForYou_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableRecommendedForYou, storeScope);
            model.RecommendedForYouARN_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.RecommendedForYouARN, storeScope);
            model.RecommendedForYouWidgetZoneId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.RecommendedForYouWidgetZoneId, storeScope);
            model.RecommendedForYouNumberOfItems_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.RecommendedForYouNumberOfItems, storeScope);
            model.RecommendedForYouAllowForGuestCustomer_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.RecommendedForYouAllowForGuestCustomer, storeScope);
            #endregion

            #region MostViewed
            model.EnableMostViewed_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableMostViewed, storeScope);
            model.MostViewedARN_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.MostViewedARN, storeScope);
            model.MostViewedWidgetZoneId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.MostViewedWidgetZoneId, storeScope);
            model.MostViewedNumberOfItems_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.MostViewedNumberOfItems, storeScope);
            model.MostViewedAllowForGuestCustomer_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.MostViewedAllowForGuestCustomer, storeScope);
            #endregion

            #region CustomersWhoViewedXAlsoViewed
            model.EnableCustomersWhoViewedXAlsoViewed_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableCustomersWhoViewedXAlsoViewed, storeScope);
            model.CustomersWhoViewedXAlsoViewedARN_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.CustomersWhoViewedXAlsoViewedARN, storeScope);
            model.CustomersWhoViewedXAlsoViewedWidgetZoneId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.CustomersWhoViewedXAlsoViewedWidgetZoneId, storeScope);
            model.CustomersWhoViewedXAlsoViewedNumberOfItems_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.CustomersWhoViewedXAlsoViewedNumberOfItems, storeScope);
            model.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer, storeScope);
            #endregion

            #region BestSellers
            model.EnableBestSellers_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableBestSellers, storeScope);
            model.BestSellersARN_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.BestSellersARN, storeScope);
            model.BestSellersWidgetZoneId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.BestSellersWidgetZoneId, storeScope);
            model.BestSellersNumberOfItems_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.BestSellersNumberOfItems, storeScope);
            model.BestSellersAllowForGuestCustomer_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.BestSellersAllowForGuestCustomer, storeScope);
            #endregion

            #region FrequentlyBoughtTogether
            model.EnableFrequentlyBoughtTogether_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.EnableFrequentlyBoughtTogether, storeScope);
            model.FrequentlyBoughtTogetherARN_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.FrequentlyBoughtTogetherARN, storeScope);
            model.FrequentlyBoughtTogetherWidgetZoneId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.FrequentlyBoughtTogetherWidgetZoneId, storeScope);
            model.FrequentlyBoughtTogetherNumberOfItems_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.FrequentlyBoughtTogetherNumberOfItems, storeScope);
            model.FrequentlyBoughtTogetherAllowForGuestCustomer_OverrideForStore = await _settingService.SettingExistsAsync(amazonPersonalizeSettings, settings => settings.FrequentlyBoughtTogetherAllowForGuestCustomer, storeScope);
            #endregion


            return model;
        }

        #endregion Methods
    }
}