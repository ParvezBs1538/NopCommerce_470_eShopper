using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record WebApiNotificationTemplateModel : BaseNopEntityModel, ILocalizedModel<WebApiNotificationTemplateLocalizedModel>, IStoreMappingSupportedModel
    {
        public WebApiNotificationTemplateModel()
        {
            AvailableActionTypes = new List<SelectListItem>();
            Locales = new List<WebApiNotificationTemplateLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            AvailableTemplates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Body")]
        public string Body { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ImageId")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.SendImmediately")]
        public bool SendImmediately { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.DelayBeforeSend")]
        [UIHint("Int32Nullable")]
        public int? DelayBeforeSend { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ActionType")]
        public int ActionTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.ActionValue")]
        public string ActionValue { get; set; }

        public int DelayPeriodId { get; set; }

        public IList<SelectListItem> AvailableActionTypes { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableTemplates { get; set; }

        public IList<WebApiNotificationTemplateLocalizedModel> Locales { get; set; }
    }

    public partial class WebApiNotificationTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.Fields.Body")]
        public string Body { get; set; }
    }
}
