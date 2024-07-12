using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public record DHLOrderModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.CustomOrderNumber")]
        public string CustomOrderNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.StoreName")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.CustomerFullName")]
        public string CustomerFullName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.OrderStatus")]
        public string OrderStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.OrderStatus")]
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.PaymentStatus")]
        public string PaymentStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.PaymentStatus")]
        public int PaymentStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.ShippingStatus")]
        public string ShippingStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.HasShippingLabel")]
        public bool HasShippingLabel { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.CanBookPickup")]
        public bool CanBookPickup { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.AirwayBillNumber")]
        public string AirwayBillNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.ConfirmationNumber")]
        public string ConfirmationNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.ReadyByTime")]
        public DateTime? ReadyByTime { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Orders.Fields.NextPickupDate")]
        public DateTime? NextPickupDate { get; set; }
    }
}
