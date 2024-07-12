using Nop.Core.Domain.Attributes;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class SurveyAttributeValue : BaseAttributeValue, ILocalizedEntity
    {
        public int SurveyAttributeMappingId { get; set; }

        public string ColorSquaresRgb { get; set; }

        public int ImageSquaresPictureId { get; set; }
    }
}
