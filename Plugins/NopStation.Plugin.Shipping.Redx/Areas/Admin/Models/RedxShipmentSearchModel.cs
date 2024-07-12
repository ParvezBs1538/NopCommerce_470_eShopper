using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Models
{
    public record RedxShipmentSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.List.SearchOrderId")]
        public string SearchOrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.List.SearchTrackingId")]
        public string SearchTrackingId { get; set; }
    }
}