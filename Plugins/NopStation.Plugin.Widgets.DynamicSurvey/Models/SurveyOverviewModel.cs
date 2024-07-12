using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Models
{
    public record SurveyOverviewModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string SystemName { get; set; }

        public string SeName { get; set; }
    }
}
