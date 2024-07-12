using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Factories
{
    public class AlgoliaSearchModelFactory : IAlgoliaSearchModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAlgoliaUpdatableItemService _algoliaUpdatableItemService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public AlgoliaSearchModelFactory(IDateTimeHelper dateTimeHelper,
            IAlgoliaUpdatableItemService algoliaUpdatableItemService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            ICategoryService categoryService,
            ISettingService settingService,
            IProductService productService,
            IVendorService vendorService,
            IStoreContext storeContext)
        {
            _dateTimeHelper = dateTimeHelper;
            _algoliaUpdatableItemService = algoliaUpdatableItemService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _customerService = customerService;
            _settingService = settingService;
            _productService = productService;
            _vendorService = vendorService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilities

        protected async Task<string> GetEntityItemNameAsync(string entityName, int entityId)
        {
            switch (entityName)
            {
                case "Product":
                    var product = await _productService.GetProductByIdAsync(entityId);
                    return product?.Name;
                case "Category":
                    var category = await _categoryService.GetCategoryByIdAsync(entityId);
                    return category?.Name;
                case "Manufacturer":
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(entityId);
                    return manufacturer?.Name;
                case "Vendor":
                    var vendor = await _vendorService.GetVendorByIdAsync(entityId);
                    return vendor?.Name;
                default:
                    return "";
            }
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var algoliaSearchSettings = await _settingService.LoadSettingAsync<AlgoliaSearchSettings>(storeScope);

            var model = algoliaSearchSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            model.CanClearOrUpdateIndex = !string.IsNullOrWhiteSpace(model.ApplicationId) && !string.IsNullOrWhiteSpace(model.AdminKey);
            var availablePositionItems = await AlgoliaSortingEnum.NameAsc.ToSelectListAsync(false);
            foreach (var positionItem in availablePositionItems.Where(x => x.Value != "0"))
                model.AvailableSortOptions.Add(positionItem);

            model.AvailableViewModes.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.ViewMode.Grid"),
                Value = "grid"
            });
            model.AvailableViewModes.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.ViewMode.List"),
                Value = "list"
            });

            if (storeScope <= 0)
                return model;

            model.AdminKey_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AdminKey, storeScope);
            model.AllowAttributeFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowAttributeFilter, storeScope);
            model.AllowCategoryFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowCategoryFilter, storeScope);
            model.AllowCustomersToSelectPageSize_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowCustomersToSelectPageSize, storeScope);
            model.AllowedSortingOptions_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowedSortingOptions, storeScope);
            model.AllowManufacturerFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowManufacturerFilter, storeScope);
            model.AllowPriceRangeFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowPriceRangeFilter, storeScope);
            model.AllowProductSorting_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowProductSorting, storeScope);
            model.AllowProductViewModeChanging_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowProductViewModeChanging, storeScope);
            model.AllowRatingFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowRatingFilter, storeScope);
            model.AllowSpecificationFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowSpecificationFilter, storeScope);
            model.AllowVendorFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AllowVendorFilter, storeScope);
            model.ApplicationId_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.ApplicationId, storeScope);
            model.AutoCompleteListSize_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.AutoCompleteListSize, storeScope);
            model.DefaultViewMode_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.DefaultViewMode, storeScope);
            model.EnableAutoComplete_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.EnableAutoComplete, storeScope);
            model.HidePoweredByAlgolia_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.HidePoweredByAlgolia, storeScope);
            model.MaximumAttributesShowInFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.MaximumAttributesShowInFilter, storeScope);
            model.MaximumCategoriesShowInFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.MaximumCategoriesShowInFilter, storeScope);
            model.MaximumManufacturersShowInFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.MaximumManufacturersShowInFilter, storeScope);
            model.MaximumSpecificationsShowInFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.MaximumVendorsShowInFilter, storeScope);
            model.MaximumVendorsShowInFilter_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.MaximumVendorsShowInFilter, storeScope);
            model.MonitoringKey_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.MonitoringKey, storeScope);
            model.SearchOnlyKey_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.SearchOnlyKey, storeScope);
            model.SearchPagePageSizeOptions_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.SearchPagePageSizeOptions, storeScope);
            model.SearchPageProductsPerPage_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.SearchTermMinimumLength, storeScope);
            model.SearchTermMinimumLength_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.SearchTermMinimumLength, storeScope);
            model.ShowProductImagesInSearchAutoComplete_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.ShowProductImagesInSearchAutoComplete, storeScope);
            model.ShowProductsCount_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.ShowProductsCount, storeScope);
            model.UsageKey_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.UsageKey, storeScope);
            model.EnableMultilingualSearch_OverrideForStore = await _settingService.SettingExistsAsync(algoliaSearchSettings, x => x.EnableMultilingualSearch, storeScope);

            return model;
        }

        public async Task<UpdatableItemListModel> PrepareUpdatableItemListModelAsync(UpdatableItemSearchModel searchModel)
        {
            var items = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync(searchModel.EntityName, searchModel.Page - 1, searchModel.PageSize);
            var model = await new UpdatableItemListModel().PrepareToGridAsync(searchModel, items, () =>
            {
                return items.SelectAwait(async item =>
                {
                    var m = item.ToModel<UpdatableItemModel>();
                    m.Name = await GetEntityItemNameAsync(searchModel.EntityName, item.EntityId);
                    m.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(item.UpdatedOnUtc, DateTimeKind.Utc);

                    var customer = await _customerService.GetCustomerByIdAsync(item.LastUpdatedBy);
                    if (customer != null)
                        m.UpdatedByCustomerName = customer.Email;
                    return m;
                });
            });

            return model;
        }

        #endregion
    }
}
