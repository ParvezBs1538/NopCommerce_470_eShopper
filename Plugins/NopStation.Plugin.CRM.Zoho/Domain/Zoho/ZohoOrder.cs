using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.CRM.Zoho.Domain.Zoho
{
    public class ZohoOrder : BaseZohoEntity
    {
        public string CustomerCurrencyCode { get; set; }

        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public decimal OrderTax { get; set; }

        public decimal CurrencyRate { get; set; }

        public int BillingAddressId { get; set; }

        public int? ShippingAddressId { get; set; }

        public string ShippingMethod { get; set; }

        public int OrderStatusId { get; set; }

        public OrderStatus OrderStatus { get => (OrderStatus)OrderStatusId; }

        public decimal OrderTotal { get; set; }

        public decimal OrderSubtotalExclTax { get; set; }

        public decimal OrderDiscount { get; set; }
    }
}
