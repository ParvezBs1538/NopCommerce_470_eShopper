using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record WebApiQueuedNotificationModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Customer")]
        public int? CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ImageUrl")]
        public string ImageUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DeviceType")]
        public string DeviceTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DeviceType")]
        public int DeviceTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentOn")]
        public DateTime? SentOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.DontSendBeforeDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? DontSendBeforeDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SendImmediately")]
        public bool SendImmediately { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ErrorLog")]
        public string ErrorLog { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SentTries")]
        public int SentTries { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.AppDeviceId")]
        public int AppDeviceId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionType")]
        public int ActionTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionType")]
        public string ActionTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.ActionValue")]
        public string ActionValue { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Fields.SubscriptionId")]
        public string SubscriptionId { get; set; }
    }
}
