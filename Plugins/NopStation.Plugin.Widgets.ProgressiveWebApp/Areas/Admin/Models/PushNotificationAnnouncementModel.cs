using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models
{
    public record PushNotificationAnnouncementModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.UseDefaultIcon")]
        public bool UseDefaultIcon { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.IconId")]
        public int IconId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.ImageId")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationAnnouncements.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
