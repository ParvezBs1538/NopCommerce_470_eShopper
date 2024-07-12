using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.AlgoliaSearch
{
    public class AlgoliaSearchSettings : ISettings
    {
        public AlgoliaSearchSettings()
        {
            AllowedSortingOptions = new List<int>();
        }

        #region Keys
        public string ApplicationId { get; set; }
        public string SearchOnlyKey { get; set; }
        public string AdminKey { get; set; }
        public string MonitoringKey { get; set; }
        public string UsageKey { get; set; }
        #endregion

        #region Search field
        public bool EnableAutoComplete { get; set; }
        public int AutoCompleteListSize { get; set; }
        public bool EnableMultilingualSearch { get; set; }
        public int SearchTermMinimumLength { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public bool AllowCustomersToSelectPageSize { get; set; }
        public string SearchPagePageSizeOptions { get; set; }
        public int SearchPageProductsPerPage { get; set; }
        public bool AllowProductViewModeChanging { get; set; }
        public string DefaultViewMode { get; set; }
        public bool HidePoweredByAlgolia { get; set; }
        #endregion

        #region Filters
        public bool AllowCategoryFilter { get; set; }
        public int MaximumCategoriesShowInFilter { get; set; }
        public bool AllowVendorFilter { get; set; }
        public int MaximumVendorsShowInFilter { get; set; }
        public bool AllowManufacturerFilter { get; set; }
        public int MaximumManufacturersShowInFilter { get; set; }
        public bool AllowSpecificationFilter { get; set; }
        public int MaximumSpecificationsShowInFilter { get; set; }
        public bool AllowAttributeFilter { get; set; }
        public int MaximumAttributesShowInFilter { get; set; }
        public bool AllowProductSorting { get; set; }
        public List<int> AllowedSortingOptions { get; set; }
        public bool AllowPriceRangeFilter { get; set; }
        public bool AllowRatingFilter { get; set; }
        public bool ShowProductsCount { get; set; }
        #endregion
    }
}
