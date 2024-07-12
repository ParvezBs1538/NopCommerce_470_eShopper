using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record CustomerAbandonmentInfoModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.NotificationSentFrequency")]
        public int NotificationSentFrequency { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Status")]
        public string StatusName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Token")]
        public string Token { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.LastNotificationSentOn")]
        public DateTime? LastNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.UnsubscribedOnUtc")]
        public DateTime? UnsubscribedOnUtc { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.TotalItems")]
        public int TotalItems { get; set; }

    }
}
