using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models
{
    public record CourierShipmentSearchModel : BaseSearchModel
    {
        public CourierShipmentSearchModel()
        {
            AvailableShippers = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.List.SearchOrderId")]
        public int? SearchOrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.List.SearchCustomOrderNumber")]
        public string SearchCustomOrderNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.List.SearchShipperId")]
        public int? SearchShipperId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentId")]
        public int? SearchShipmentId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentTrackingNumber")]
        public string SearchShipmentTrackingNumber { get; set; }

        public IList<SelectListItem> AvailableShippers { get; set; }
    }
}
