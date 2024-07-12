using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public class WebApiModelFactory : IWebApiModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IApiStringResourceService _apiStringResourceService;
        private readonly IWorkContext _workContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public WebApiModelFactory(ISettingService settingService,
            IStoreContext storeContext,
            IApiStringResourceService apiStringResourceService,
            IWorkContext workContext,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _apiStringResourceService = apiStringResourceService;
            _workContext = workContext;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Utilities

        protected async Task PrepareSliderTypesAsync(IList<SelectListItem> items, bool excludeDefaultItem = false, string label = "")
        {
            var selectList = await BarcodeScanKeyType.Sku.ToSelectListAsync(false);
            foreach (var item in selectList)
                items.Add(item);
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var webApiSettings = await _settingService.LoadSettingAsync<WebApiSettings>(storeScope);

            var model = webApiSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            model.SearchLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            model.LocaleResourceSearchModel.LanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            await _baseAdminModelFactory.PrepareLanguagesAsync(model.AvailableLanguages, false);
            await PrepareSliderTypesAsync(model.AvailableBarcodeScanKeys, true);

            if (storeScope == 0)
                return model;

            model.CheckIat_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.CheckIat, storeScope);
            model.IOSProductPriceTextSize_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSProductPriceTextSize, storeScope);
            model.IonicProductPriceTextSize_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IonicProductPriceTextSize, storeScope);
            model.AndroidProductPriceTextSize_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndroidProductPriceTextSize, storeScope);
            model.ShowHomepageSlider_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowHomepageSlider, storeScope);
            model.SliderAutoPlay_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.SliderAutoPlay, storeScope);
            model.SliderAutoPlayTimeout_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.SliderAutoPlayTimeout, storeScope);
            model.EnableJwtSecurity_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.EnableJwtSecurity, storeScope);
            model.MaximumNumberOfHomePageSliders_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.MaximumNumberOfHomePageSliders, storeScope);
            model.NumberOfHomepageCategoryProducts_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.NumberOfHomepageCategoryProducts, storeScope);
            model.NumberOfManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.NumberOfManufacturers, storeScope);
            model.ShowBestsellersOnHomepage_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowBestsellersOnHomepage, storeScope);
            model.NumberOfBestsellersOnHomepage_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.NumberOfBestsellersOnHomepage, storeScope);
            model.ShowFeaturedProducts_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowFeaturedProducts, storeScope);
            model.ShowHomepageCategoryProducts_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowHomepageCategoryProducts, storeScope);
            model.ShowManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowManufacturers, storeScope);
            model.ShowSubCategoryProducts_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowSubCategoryProducts, storeScope);
            model.TokenKey_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenKey, storeScope);
            model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.SecretKey, storeScope);
            model.TokenSecondsValid_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenSecondsValid, storeScope);
            model.TokenSecret_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenSecret, storeScope);
            model.AndroidVersion_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndroidVersion, storeScope);
            model.AndriodForceUpdate_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndriodForceUpdate, storeScope);
            model.PlayStoreUrl_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.PlayStoreUrl, storeScope);
            model.IOSVersion_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSVersion, storeScope);
            model.IOSForceUpdate_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSForceUpdate, storeScope);
            model.AppStoreUrl_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AppStoreUrl, storeScope);
            model.LogoId_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.LogoId, storeScope);
            model.LogoSize_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.LogoSize, storeScope);
            model.ShowChangeBaseUrlPanel_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowChangeBaseUrlPanel, storeScope);
            model.PrimaryThemeColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.PrimaryThemeColor, storeScope);
            model.BottomBarActiveColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.BottomBarActiveColor, storeScope);
            model.BottomBarBackgroundColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.BottomBarBackgroundColor, storeScope);
            model.BottomBarInactiveColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.BottomBarInactiveColor, storeScope);
            model.TopBarBackgroundColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TopBarBackgroundColor, storeScope);
            model.TopBarTextColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TopBarTextColor, storeScope);
            model.GradientStartingColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.GradientStartingColor, storeScope);
            model.GradientMiddleColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.GradientMiddleColor, storeScope);
            model.GradientEndingColor_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.GradientEndingColor, storeScope);
            model.GradientEnabled_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.GradientEnabled, storeScope);
            model.ProductBarcodeScanKeyId_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ProductBarcodeScanKeyId, storeScope);
            model.AllowCustomersToDeleteAccount_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AllowCustomersToDeleteAccount, storeScope);

            return model;
        }

        public async Task<LocaleResourceListModel> PrepareLocaleResourceListModelAsync(LocaleResourceSearchModel searchModel, Language language)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (language == null)
                throw new ArgumentNullException(nameof(language));

            //get locale resources
            var localeResources = (await _apiStringResourceService.GetAllResourceValuesAsync(language.Id))
                .OrderBy(localeResource => localeResource.Key).AsQueryable();

            //filter locale resources
            //TODO: move filter to language service
            if (!string.IsNullOrEmpty(searchModel.SearchResourceName))
                localeResources = localeResources.Where(l => l.Key.ToLowerInvariant().Contains(searchModel.SearchResourceName.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(searchModel.SearchResourceValue))
                localeResources = localeResources.Where(l => l.Value.Value.ToLowerInvariant().Contains(searchModel.SearchResourceValue.ToLowerInvariant()));

            var pagedLocaleResources = new PagedList<KeyValuePair<string, KeyValuePair<string, string>>>(localeResources.ToList(),
                searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new LocaleResourceListModel().PrepareToGrid(searchModel, pagedLocaleResources, () =>
            {
                //fill in model values from the entity
                return pagedLocaleResources.Select(localeResource => new LocaleResourceModel
                {
                    Id = Convert.ToInt32(localeResource.Value.Key.Split("__")[0]),
                    LanguageId = language.Id,
                    CombinedId = localeResource.Value.Key,
                    ResourceName = localeResource.Key,
                    ResourceValue = localeResource.Value.Value
                });
            });

            return model;
        }

        #endregion
    }
}
