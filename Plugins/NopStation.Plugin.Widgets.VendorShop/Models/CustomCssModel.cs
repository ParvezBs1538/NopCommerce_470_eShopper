using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Models
{
    public record CustomCssModel : BaseNopModel
    {
        public string Css { get; set; }
    }
}
