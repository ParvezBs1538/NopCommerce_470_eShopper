using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveyAttributeValueSearchModel : BaseSearchModel
    {
        #region Properties

        public int SurveyAttributeMappingId { get; set; }

        #endregion
    }
}