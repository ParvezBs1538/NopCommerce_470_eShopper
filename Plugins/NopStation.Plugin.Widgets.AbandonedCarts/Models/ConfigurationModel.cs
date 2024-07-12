using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }
        public bool IsEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.AbandonmentCutOffTime")]
        public int AbandonmentCutOffTime { get; set; }
        public bool AbandonmentCutOffTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.NotificationSendingCondition")]
        public int NotificationSendingConditionId { get; set; }
        public bool NotificationSendingCondition_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.IsEnabledFirstNotification")]
        public bool IsEnabledFirstNotification { get; set; }
        public bool IsEnabledFirstNotification_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.DurationAfterFirstAbandonment")]
        public int DurationAfterFirstAbandonment { get; set; }
        public bool DurationAfterFirstAbandonment_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.IsEnabledSecondNotification")]
        public bool IsEnabledSecondNotification { get; set; }
        public bool IsEnabledSecondNotification_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.DurationAfterSecondAbandonment")]
        public int DurationAfterSecondAbandonment { get; set; }
        public bool DurationAfterSecondAbandonment_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.DontSendSameDay")]
        public bool DontSendSameDay { get; set; }
        public bool DontSendSameDay_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.CustomerOnlineCutoffTime")]
        public int CustomerOnlineCutoffTime { get; set; }
        public bool CustomerOnlineCutoffTime_OverrideForStore { get; set; }
    }
}