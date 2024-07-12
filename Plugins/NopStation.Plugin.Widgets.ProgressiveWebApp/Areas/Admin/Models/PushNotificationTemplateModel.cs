using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models
{
    public record PushNotificationTemplateModel : BaseNopEntityModel, ILocalizedModel<PushNotificationTemplateLocalizedModel>, IStoreMappingSupportedModel
    {
        public PushNotificationTemplateModel()
        {
            Locales = new List<PushNotificationTemplateLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.UseDefaultIcon")]
        public bool UseDefaultIcon { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.IconId")]
        public int IconId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.ImageId")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Active")]
        public bool Active { get; set; }

        public string AllowedTokens { get; set; }

        public IList<PushNotificationTemplateLocalizedModel> Locales { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }


    public partial class PushNotificationTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.PushNotificationTemplates.Fields.Body")]
        public string Body { get; set; }
    }
}
