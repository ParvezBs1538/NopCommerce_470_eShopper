using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record AbandonedCartModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.ShoppingCartItem")]
        public int ShoppingCartItemId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.OrderItem")]
        public int OrderItemId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.StatusChangedOn")]
        public DateTime StatusChangedOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.FirstNotificationSentOn")]
        public DateTime FirstNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.SecondNotificationSentOn")]
        public DateTime SecondNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.FirstNotificationMessageQueueId")]
        public int FirstNotificationMessageQueueId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.SecondNotificationMessageQueueId")]
        public int SecondNotificationMessageQueueId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.IsSoftDeleted")]
        public bool IsSoftDeleted { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Quantity")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.UnitPrice")]
        public string UnitPrice { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.TotalPrice")]
        public string TotalPrice { get; set; }
        public string AttributeInfo { get; set; }
    }
}