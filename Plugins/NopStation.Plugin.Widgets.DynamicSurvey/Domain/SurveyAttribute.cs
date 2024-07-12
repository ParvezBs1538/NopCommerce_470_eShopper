using Nop.Core.Domain.Attributes;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class SurveyAttribute : BaseAttribute, ILocalizedEntity
    {
        public string Description { get; set; }
    }
}
