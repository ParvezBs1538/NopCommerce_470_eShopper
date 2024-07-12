using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeSurveyModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.UsedBySurveys.Survey")]
        public string SurveyName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveyAttributes.UsedBySurveys.Published")]
        public bool Published { get; set; }

        #endregion
    }
}