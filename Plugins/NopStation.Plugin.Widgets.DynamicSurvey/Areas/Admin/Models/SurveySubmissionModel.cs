using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveySubmissionModel : BaseNopEntityModel
    {
        public int SurveyId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.SurveyName")]
        public string SurveyName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.Attribute")]
        public string AttributeXml { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.Attribute")]
        public string AttributeDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CustomerEmail")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}