using System.Collections.Generic;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterProductTagsModel
    {
        public FilterProductTagsModel()
        {
            ProductTags = new List<ProductTagsModel>();
        }

        public FiltersUI CheckOrDropdown { get; set; }

        public int MaxManufacturersToDisplay { get; set; }
        public IList<ProductTagsModel> ProductTags { get; set; }
    }
}
