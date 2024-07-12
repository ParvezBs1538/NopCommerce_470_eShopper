using System;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OrderRatings.Models
{
    public record OrderRatingModel : BaseNopEntityModel
    {
        public int OrderId { set; get; }
        public int Rating { set; get; }
        public string Note { get; set; }
        public DateTime? RatedOn { get; set; }
        public bool ShowOrderRatedDateInDetailsPage { get; set; }
        public string CustomOrderNumber { get; set; }
        public string OrderTotal { get; set; }
        public OrderStatus OrderStatusEnum { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string ShippingStatus { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
