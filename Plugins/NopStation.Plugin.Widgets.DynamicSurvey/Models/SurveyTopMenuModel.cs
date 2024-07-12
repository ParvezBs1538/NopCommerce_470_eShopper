using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Models
{
    public record SurveyTopMenuModel
    {
        public SurveyTopMenuModel()
        {
            Surveys = new List<SurveyModel>();
        }

        public string WidgetZone { get; set; }

        public IList<SurveyModel> Surveys { get; set; }

        public record SurveyModel : BaseNopEntityModel
        {
            public string Name { get; set; }
            public string SeName { get; set; }
        }
    }
}
