using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Misc.WebApiNotification.Domains
{
    public class WebApiNotificationCampaign : BaseEntity, ILocalizedEntity, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public int ImageId { get; set; }

        public DateTime SendingWillStartOnUtc { get; set; }

        public DateTime? AddedToQueueOnUtc { get; set; }

        public int LimitedToStoreId { get; set; }

        public bool Deleted { get; set; }

        public string CustomerRoles { get; set; }

        public string DeviceTypes { get; set; }

        public int ActionTypeId { get; set; }

        public string ActionValue { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public NotificationActionType ActionType
        {
            get => (NotificationActionType)ActionTypeId;
            set => ActionTypeId = (int)value;
        }

        public WebApiNotificationCampaign Clone()
        {
            var campaign = new WebApiNotificationCampaign()
            {
                Body = Body,
                ImageId = ImageId,
                LimitedToStoreId = LimitedToStoreId,
                Name = Name,
                SendingWillStartOnUtc = SendingWillStartOnUtc,
                Title = Title,
                CustomerRoles = CustomerRoles,
                DeviceTypes = DeviceTypes,
                ActionValue = ActionValue,
                ActionTypeId = ActionTypeId
            };

            return campaign;
        }
    }
}
