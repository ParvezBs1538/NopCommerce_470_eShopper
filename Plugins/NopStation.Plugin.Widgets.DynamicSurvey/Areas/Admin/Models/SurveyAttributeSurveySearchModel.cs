using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeSurveySearchModel : BaseSearchModel
    {
        #region Properties

        public int SurveyAttributeId { get; set; }

        #endregion
    }
}