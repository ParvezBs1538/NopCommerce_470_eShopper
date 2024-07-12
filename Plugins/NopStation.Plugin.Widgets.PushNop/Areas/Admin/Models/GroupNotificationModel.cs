using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public record GroupNotificationModel : BaseNopEntityModel, ILocalizedModel<GroupNotificationLocalizedModel>
    {
        public GroupNotificationModel()
        {
            AvailableSmartGroups = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            Locales = new List<GroupNotificationLocalizedModel>();
            CopySmartGroupNotificationModel = new CopySmartGroupNotificationModel();
        }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.UseDefaultIcon")]
        public bool UseDefaultIcon { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.IconId")]
        public int IconId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.ImageId")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Subscriptions")]
        public int Subscriptions { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.SendToAll")]
        public bool SendToAll { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.SmartGroup")]
        public int? SmartGroupId { get; set; }
        public string SmartGroupName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.AddedToQueueOn")]
        public DateTime? AddedToQueueOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.SendingWillStartOn")]
        [UIHint("DateTime")]
        public DateTime SendingWillStartOn { get; set; }

        public IList<GroupNotificationLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.LimitedToStoreId")]
        public int LimitedToStoreId { get; set; }

        public IList<SelectListItem> AvailableSmartGroups { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public CopySmartGroupNotificationModel CopySmartGroupNotificationModel { get; set; }
    }

    public partial class GroupNotificationLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PushNop.GroupNotifications.Fields.Body")]
        public string Body { get; set; }
    }
}
