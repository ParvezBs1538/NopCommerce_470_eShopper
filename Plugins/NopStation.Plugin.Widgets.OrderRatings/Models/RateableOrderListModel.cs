using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OrderRatings.Models
{
    public record RateableOrderListModel : BaseNopModel
    {
        public RateableOrderListModel()
        {
            Orders = new List<OrderDetailsModel>();
        }

        public IList<OrderDetailsModel> Orders { get; set; }

        public partial record OrderDetailsModel : BaseNopEntityModel
        {
            public string CustomOrderNumber { get; set; }
            public string OrderTotal { get; set; }
            public OrderStatus OrderStatusEnum { get; set; }
            public string OrderStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string ShippingStatus { get; set; }
            public DateTime CreatedOn { get; set; }
        }
    }
}
