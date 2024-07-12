using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductRibbon.Models
{
    public record ProductRibbonModel : BaseNopModel
    {
        public string Discount { get; set; }

        public bool IsNew { get; set; }

        public bool IsBestSeller { get; set; }
    }
}