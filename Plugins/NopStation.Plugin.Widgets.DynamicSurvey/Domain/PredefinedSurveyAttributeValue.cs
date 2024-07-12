using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class PredefinedSurveyAttributeValue : BaseEntity, ILocalizedEntity
    {
        public int SurveyAttributeId { get; set; }

        public string Name { get; set; }

        public bool IsPreSelected { get; set; }

        public int DisplayOrder { get; set; }
    }
}
