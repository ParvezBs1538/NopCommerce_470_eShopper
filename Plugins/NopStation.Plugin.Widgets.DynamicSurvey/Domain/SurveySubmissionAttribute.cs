using Nop.Core;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class SurveySubmissionAttribute : BaseEntity
    {
        public int SurveySubmissionId { get; set; }

        public int SurveyAttributeMappingId { get; set; }

        public string AttributeValue { get; set; }
    }
}
