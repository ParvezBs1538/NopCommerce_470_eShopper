using System.Collections.Generic;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterProductVariantAttributesModel
    {
        public FilterProductVariantAttributesModel()
        {
            ProductVariantAttributesOptions = new List<ProductVariantAttributesOptionsModel>();
        }
        public IList<ProductVariantAttributesOptionsModel> ProductVariantAttributesOptions { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
