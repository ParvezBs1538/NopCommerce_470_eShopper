using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Domains
{
    public class PushNotificationAnnouncement : BaseEntity
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public bool UseDefaultIcon { get; set; }

        public int IconId { get; set; }

        public int ImageId { get; set; }

        public string Url { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
