using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Domain
{
    public partial class SurveySubmission : BaseEntity
    {
        public int SurveyId { get; set; }

        public string AttributeXml { get; set; }

        public string AttributeDescription { get; set; }

        public int CustomerId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
