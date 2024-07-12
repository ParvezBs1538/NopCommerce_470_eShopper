using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record CopySurveyModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Copy.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Copy.Published")]
        public bool Published { get; set; }

        #endregion
    }
}