using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models
{
    public record QueuedPushNotificationModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Customer")]
        public int? CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.IconUrl")]
        public string IconUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.ImageUrl")]
        public string ImageUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.Rtl")]
        public bool Rtl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.QueuedPushNotifications.Fields.SentOn")]
        public DateTime? SentOn { get; set; }
    }
}
