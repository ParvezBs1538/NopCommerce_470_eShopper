using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
            AllowedSortingOptions = new List<int>();
            UpdateIndicesModel = new UpdateIndicesModel();
            AvailableViewModes = new List<SelectListItem>();
        }

        #region Keys

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId")]
        public string ApplicationId { get; set; }
        public bool ApplicationId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey")]
        public string SearchOnlyKey { get; set; }
        public bool SearchOnlyKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey")]
        public string AdminKey { get; set; }
        public bool AdminKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey")]
        public string MonitoringKey { get; set; }
        public bool MonitoringKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey")]
        public string UsageKey { get; set; }
        public bool UsageKey_OverrideForStore { get; set; }

        #endregion

        #region Search field

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableAutoComplete")]
        public bool EnableAutoComplete { get; set; }
        public bool EnableAutoComplete_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize")]
        public int AutoCompleteListSize { get; set; }
        public bool AutoCompleteListSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch")]
        public bool EnableMultilingualSearch { get; set; }
        public bool EnableMultilingualSearch_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchTermMinimumLength")]
        public int SearchTermMinimumLength { get; set; }
        public bool SearchTermMinimumLength_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductImagesInSearchAutoComplete")]
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public bool ShowProductImagesInSearchAutoComplete_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }
        public bool AllowCustomersToSelectPageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions")]
        public string SearchPagePageSizeOptions { get; set; }
        public bool SearchPagePageSizeOptions_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage")]
        public int SearchPageProductsPerPage { get; set; }
        public bool SearchPageProductsPerPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductViewModeChanging")]
        public bool AllowProductViewModeChanging { get; set; }
        public bool AllowProductViewModeChanging_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode")]
        public string DefaultViewMode { get; set; }
        public bool DefaultViewMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.HidePoweredByAlgolia")]
        public bool HidePoweredByAlgolia { get; set; }
        public bool HidePoweredByAlgolia_OverrideForStore { get; set; }

        #endregion

        #region Filters

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCategoryFilter")]
        public bool AllowCategoryFilter { get; set; }
        public bool AllowCategoryFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter")]
        public int MaximumCategoriesShowInFilter { get; set; }
        public bool MaximumCategoriesShowInFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowVendorFilter")]
        public bool AllowVendorFilter { get; set; }
        public bool AllowVendorFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter")]
        public int MaximumVendorsShowInFilter { get; set; }
        public bool MaximumVendorsShowInFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowManufacturerFilter")]
        public bool AllowManufacturerFilter { get; set; }
        public bool AllowManufacturerFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter")]
        public int MaximumManufacturersShowInFilter { get; set; }
        public bool MaximumManufacturersShowInFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowSpecificationFilter")]
        public bool AllowSpecificationFilter { get; set; }
        public bool AllowSpecificationFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter")]
        public int MaximumSpecificationsShowInFilter { get; set; }
        public bool MaximumSpecificationsShowInFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowAttributeFilter")]
        public bool AllowAttributeFilter { get; set; }
        public bool AllowAttributeFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumAttributesShowInFilter")]
        public int MaximumAttributesShowInFilter { get; set; }
        public bool MaximumAttributesShowInFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductSorting")]
        public bool AllowProductSorting { get; set; }
        public bool AllowProductSorting_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortingOptions")]
        public IList<int> AllowedSortingOptions { get; set; }
        public bool AllowedSortingOptions_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowPriceRangeFilter")]
        public bool AllowPriceRangeFilter { get; set; }
        public bool AllowPriceRangeFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowRatingFilter")]
        public bool AllowRatingFilter { get; set; }
        public bool AllowRatingFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductsCount")]
        public bool ShowProductsCount { get; set; }
        public bool ShowProductsCount_OverrideForStore { get; set; }

        #endregion

        public int ActiveStoreScopeConfiguration { get; set; }

        public UpdateIndicesModel UpdateIndicesModel { get; set; }

        public bool CanClearOrUpdateIndex { get; set; }

        public IList<SelectListItem> AvailableViewModes { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }
    }
}
