using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record WebApiNotificationCampaignSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchKeyword")]
        public string SearchKeyword { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchSendStartFromDate")]
        public DateTime? SearchSendStartFromDate { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.List.SearchSendStartToDate")]
        public DateTime? SearchSendStartToDate { get; set; }
    }
}
