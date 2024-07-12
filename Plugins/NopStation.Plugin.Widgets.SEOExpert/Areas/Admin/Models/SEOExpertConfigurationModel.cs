using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Models
{
    public record SEOExpertConfigurationModel : BaseNopModel
    {
        public SEOExpertConfigurationModel()
        {
            RegenerateConditionIds = new List<int>();
            AvailableRegenerateConditions = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.OpenAIApiKey")]
        public string OpenAIApiKey { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Endpoint")]
        public string Endpoint { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.ModelName")]
        public string ModelName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.RequireAdminApproval")]
        public bool RequireAdminApproval { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.EnableListGeneration")]
        public bool EnableListGeneration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithName")]
        public string AdditionalInfoWithName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithShortDescription")]
        public string AdditionalInfoWithShortDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithFullDescription")]
        public string AdditionalInfoWithFullDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Temperature")]
        public decimal Temperature { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.RegenerateConditionId")]
        public IList<int> RegenerateConditionIds { get; set; }
        public IList<SelectListItem> AvailableRegenerateConditions { get; set; }
    }
}