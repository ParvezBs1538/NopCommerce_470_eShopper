using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public record FilterPriceRangeModel : BaseNopModel
    {
        public string CurrencySymbol { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal CurrentMinPrice { get; set; }
        public decimal CurrentMaxPrice { get; set; }
    }
}
