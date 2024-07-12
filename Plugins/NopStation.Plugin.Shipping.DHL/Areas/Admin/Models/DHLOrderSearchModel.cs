using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public record DHLOrderSearchModel : BaseSearchModel
    {
        public int OrderId { get; set; }
    }
}
