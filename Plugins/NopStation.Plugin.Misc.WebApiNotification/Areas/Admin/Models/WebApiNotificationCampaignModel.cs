using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record WebApiNotificationCampaignModel : BaseNopEntityModel, ILocalizedModel<WebApiNotificationCampaignLocalizedModel>
    {
        public WebApiNotificationCampaignModel()
        {
            AvailableActionTypes = new List<SelectListItem>();
            AvailableSmartGroups = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
            AvailableDeviceTypes = new List<SelectListItem>();
            CustomerRoles = new List<int>();
            DeviceTypes = new List<int>();
            CopyPushNotificationCampaignModel = new CopyPushNotificationCampaignModel();
            Locales = new List<WebApiNotificationCampaignLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Body")]
        public string Body { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ImageId")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.AddedToQueueOn")]
        public DateTime? AddedToQueueOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.SendingWillStartOn")]
        [UIHint("DateTime")]
        public DateTime SendingWillStartOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.CustomerRoles")]
        public IList<int> CustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.DeviceTypes")]
        public IList<int> DeviceTypes { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ActionType")]
        public int ActionTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.ActionValue")]
        public string ActionValue { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.LimitedToStoreId")]
        public int LimitedToStoreId { get; set; }

        public IList<SelectListItem> AvailableActionTypes { get; set; }
        public IList<SelectListItem> AvailableSmartGroups { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        public IList<SelectListItem> AvailableDeviceTypes { get; set; }

        public CopyPushNotificationCampaignModel CopyPushNotificationCampaignModel { get; set; }

        public IList<WebApiNotificationCampaignLocalizedModel> Locales { get; set; }
    }

    public partial class WebApiNotificationCampaignLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationCampaigns.Fields.Body")]
        public string Body { get; set; }
    }
}
