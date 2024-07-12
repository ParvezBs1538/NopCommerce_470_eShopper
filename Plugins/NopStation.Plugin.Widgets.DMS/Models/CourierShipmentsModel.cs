using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.UI.Paging;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record CourierShipmentsModel : BasePageableModel
    {
        public CourierShipmentsModel()
        {
            AvailableFilterOptions = new List<SelectListItem>();
            AvailableShippingStatusOptions = new List<SelectListItem>();
            CourierShipments = new List<CourierShipmentOverviewModel>();
            PageSizeOptions = new List<SelectListItem>();
        }

        [NopResourceDisplayName("NopStation.DMS.Shipments.List.FilterOptionId")]
        public int FilterOptionId { get; set; }

        public bool UseAjaxLoading { get; set; }

        public string WarningMessage { get; set; }

        public string NoResultMessage { get; set; }

        public bool InvalidAccount { get; set; }

        public bool AllowShippersToSelectPageSize { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.List.ShipmentPageSize")]
        public int ShipmentPageSize { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.List.ShippingStatusId")]
        public int ShippingStatusId { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.List.CourierShipmentStatusId")]
        public int? CourierShipmentStatusId { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.List.TrackingNumber")]
        public string TrackingNumber { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.List.Email")]
        public string Email { get; set; }

        public DateTime? CreatedOnUtc { get; internal set; }

        public IList<CourierShipmentOverviewModel> CourierShipments { get; set; }
        public IList<SelectListItem> AvailableFilterOptions { get; set; }
        public IList<SelectListItem> PageSizeOptions { get; set; }
        public IList<SelectListItem> AvailableShippingStatusOptions { get; set; }
    }
}
