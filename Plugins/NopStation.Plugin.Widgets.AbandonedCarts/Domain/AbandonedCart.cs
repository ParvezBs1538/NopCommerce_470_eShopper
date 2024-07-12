using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Domain
{
    public class AbandonedCart : BaseEntity
    {
        public int CustomerId { get; set; }
        public int ShoppingCartItemId { get; set; }
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }
        public int StatusId { get; set; }
        public DateTime StatusChangedOn { get; set; }
        public bool IsSoftDeleted { get; set; }
        public DateTime FirstNotificationSentOn { get; set; }
        public DateTime SecondNotificationSentOn { get; set; }
        public int FirstNotificationMessageQueueId { get; set; }
        public int SecondNotificationMessageQueueId { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; }
        public string TotalPrice { get; set; }
        public string AttributeInfo { get; set; }
    }
}
