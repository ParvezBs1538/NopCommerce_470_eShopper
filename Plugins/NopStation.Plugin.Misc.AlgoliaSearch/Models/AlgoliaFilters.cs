using System.Collections.Generic;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Models
{
    public class AlgoliaFilters
    {
        public AlgoliaFilters()
        {
            AvailableVendors = new List<FilterItemModel>();
            AvailableManufacturers = new List<FilterItemModel>();
            AvailableCategories = new List<FilterItemModel>();
            AvailableSpecifications = new List<FilterItemModel>();
            AvailableRatings = new List<FilterItemModel>();
        }

        public decimal MaxPrice { get; set; }

        public decimal MinPrice { get; set; }

        public IList<FilterItemModel> AvailableVendors { get; set; }

        public IList<FilterItemModel> AvailableManufacturers { get; set; }

        public IList<FilterItemModel> AvailableCategories { get; set; }

        public IList<FilterItemModel> AvailableSpecifications { get; set; }

        public IList<FilterItemModel> AvailableRatings { get; set; }
    }
}
