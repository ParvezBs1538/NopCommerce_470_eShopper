using Nop.Core.Configuration;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;

namespace NopStation.Plugin.Widgets.AbandonedCarts
{
    /// <summary>
    /// Represents settings of "Abandoned Carts" plugin
    /// </summary>
    public class AbandonmentCartSettings : ISettings
    {
        public bool IsEnabled { get; set; }
        public int AbandonmentCutOffTime { get; set; }
        public AbandonedType NotificationSendingConditionId { get; set; }
        public bool IsEnabledFirstNotification { get; set; }
        public int DurationAfterFirstAbandonment { get; set; }
        public bool IsEnabledSecondNotification { get; set; }
        public int DurationAfterSecondAbandonment { get; set; }
        public bool DontSendSameDay { get; set; }
        public int CustomerOnlineCutoffTime { get; set; }
    }
}
