using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Domain
{
    public class CustomerAbandonmentInfo : BaseEntity
    {
        public int CustomerId { get; set; }
        public int NotificationSentFrequency { get; set; }
        public int StatusId { get; set; }
        public string Token { get; set; }
        public DateTime LastNotificationSentOn { get; set; }
        public DateTime UnsubscribedOnUtc { get; set; }
    }
}
