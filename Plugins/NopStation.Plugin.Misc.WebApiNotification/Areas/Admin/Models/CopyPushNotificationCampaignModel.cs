using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record CopyPushNotificationCampaignModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Copy.SendingWillStartOn")]
        [UIHint("DateTime")]
        public DateTime SendingWillStartOnUtc { get; set; }
    }
}
