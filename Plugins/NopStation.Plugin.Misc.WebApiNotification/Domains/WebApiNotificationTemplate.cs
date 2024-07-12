using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Misc.WebApiNotification.Domains
{
    public class WebApiNotificationTemplate : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public int ImageId { get; set; }

        public bool Active { get; set; }

        public bool LimitedToStores { get; set; }

        public bool SendImmediately { get; set; }

        public int? DelayBeforeSend { get; set; }

        public int DelayPeriodId { get; set; }

        public int ActionTypeId { get; set; }

        public string ActionValue { get; set; }

        public NotificationDelayPeriod DelayPeriod
        {
            get => (NotificationDelayPeriod)DelayPeriodId;
            set => DelayPeriodId = (int)value;
        }

        public NotificationActionType ActionType
        {
            get => (NotificationActionType)ActionTypeId;
            set => ActionTypeId = (int)value;
        }
    }
}
