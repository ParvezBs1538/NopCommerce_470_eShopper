using System;
using System.Collections.Generic;
using Nop.Web.Framework.UI.Paging;

namespace NopStation.Plugin.Widgets.AffiliateStation.Models
{
    public record AffiliatedOrderModel : BasePageableModel
    {
        public AffiliatedOrderModel()
        {
            Orders = new List<OrderModel>();
            PagingFilteringContext = new AffiliatedOrderPagingFilteringModel();
        }

        public string TotalCommission { get; set; }
        public string PayableCommission { get; set; }
        public string PaidCommission { get; set; }
        public IList<OrderModel> Orders { get; set; }
        public AffiliatedOrderPagingFilteringModel PagingFilteringContext { get; set; }

        public class OrderModel
        {
            public int OrderId { get; set; }
            public string TotalCommissionAmount { get; set; }
            public string CommissionStatus { get; set; }
            public DateTime? CommissionPaidOn { get; set; }
            public string OrderStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string PaidCommissionAmount { get; set; }
            public DateTime CreatedOn { get; set; }
        }
    }

    public record AffiliatedOrderPagingFilteringModel : BasePageableModel
    {
    }
}
