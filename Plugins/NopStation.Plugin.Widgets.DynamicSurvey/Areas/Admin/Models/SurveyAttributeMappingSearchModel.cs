using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeMappingSearchModel : BaseSearchModel
    {
        #region Properties

        public int SurveyId { get; set; }

        #endregion
    }
}