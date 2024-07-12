using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class ProductVariantAttributesOptionsModel
    {
        public ProductVariantAttributesOptionsModel()
        {
            CheckedState = CheckedState.UnChecked;
        }

        public CheckedState CheckedState { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public string ColorSquaresRgb { get; set; }
    }
}
