using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.BulkSms.Areas.Admin.Models
{
    public record SmsTemplateModel : BaseNopEntityModel, ILocalizedModel<SmsTemplateLocalizedModel>, 
        IStoreMappingSupportedModel, IAclSupportedModel
    {
        public SmsTemplateModel()
        {
            Locales = new List<SmsTemplateLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.SmsTemplates.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.SmsTemplates.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.SmsTemplates.Fields.Active")]
        public bool Active { get; set; }

        public string AllowedTokens { get; set; }

        public IList<SmsTemplateLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.SmsTemplates.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.SmsTemplates.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }

    public partial class SmsTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.BulkSms.SmsTemplates.Fields.Body")]
        public string Body { get; set; }
    }
}
