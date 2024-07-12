using System.Collections.Generic;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterProductAttributesModel
    {
        public FilterProductAttributesModel()
        {
            ProductVariantAttributes = new List<FilterProductVariantAttributesModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public FiltersUI CheckOrDropdowns { get; set; }

        public IList<FilterProductVariantAttributesModel> ProductVariantAttributes { get; set; }
    }
}
