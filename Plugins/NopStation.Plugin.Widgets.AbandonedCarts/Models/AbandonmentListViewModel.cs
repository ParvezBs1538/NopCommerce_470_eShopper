using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record AbandonmentListViewModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Product")]
        public string ProductName { get; set; }
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.ProductSku")]
        public string ProductSku { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Status")]
        public string StatusName { get; set; }
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.StatusChangedOn")]
        public DateTime StatusChangedOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.FirstNotificationSentOn")]
        public DateTime? FirstNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.SecondNotificationSentOn")]
        public DateTime? SecondNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Quantity")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.UnitPrice")]
        public string UnitPrice { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.TotalPrice")]
        public string TotalPrice { get; set; }
        public string AttributeInfo { get; set; }
    }
}
