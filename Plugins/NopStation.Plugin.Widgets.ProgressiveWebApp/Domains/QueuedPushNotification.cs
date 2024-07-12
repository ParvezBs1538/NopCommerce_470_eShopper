using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Domains
{
    public class QueuedPushNotification : BaseEntity
    {
        public int? CustomerId { get; set; }

        public int StoreId { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string IconUrl { get; set; }

        public string ImageUrl { get; set; }

        public string Url { get; set; }

        public bool Rtl { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? SentOnUtc { get; set; }
    }
}
