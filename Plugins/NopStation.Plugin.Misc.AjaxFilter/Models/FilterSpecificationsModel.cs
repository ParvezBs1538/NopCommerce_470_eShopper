using System.Collections.Generic;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterSpecificationsModel
    {
        public FilterSpecificationsModel()
        {
            SpecificationAttributes = new List<SpecificationAttributesModel>();
        }
        public FiltersUI CheckOrDropdowns { get; set; }

        public int ViewMoreSpecificiationId { get; set; }
        public int MaxDisplayForSpecificationAttributes { get; set; }
        public bool EnableSearchForSpecificationAttribute { get; set; }
        public bool EnableSearchForManufacturer { get; set; }
        public IList<SpecificationAttributesModel> SpecificationAttributes { get; set; }

        public bool ShowProductCountInFilter { get; set; }

    }
}
