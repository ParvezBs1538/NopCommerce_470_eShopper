using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.PushNop.Domains
{
    public class SmartGroupNotification : BaseEntity, ILocalizedEntity, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public bool UseDefaultIcon { get; set; }

        public int IconId { get; set; }

        public int ImageId { get; set; }

        public string Url { get; set; }

        public bool SendToAll { get; set; }

        public int? SmartGroupId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime SendingWillStartOnUtc { get; set; }

        public DateTime? AddedToQueueOnUtc { get; set; }

        public int LimitedToStoreId { get; set; }

        public bool Deleted { get; set; }

        public SmartGroupNotification Clone()
        {
            var notification = new SmartGroupNotification()
            {
                SendToAll = SendToAll,
                SmartGroupId = SmartGroupId,
                Body = Body,
                IconId = IconId,
                ImageId = ImageId,
                LimitedToStoreId = LimitedToStoreId,
                Name = Name,
                SendingWillStartOnUtc = SendingWillStartOnUtc,
                Title = Title,
                Url = Url,
                UseDefaultIcon = UseDefaultIcon
            };
            return notification;
        }
    }
}
