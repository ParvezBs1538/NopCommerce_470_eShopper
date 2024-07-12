using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(Survey), "NS_DF_Survey" },
            { typeof(SurveyAttribute), "NS_DF_SurveyAttribute" },
            { typeof(SurveyAttributeMapping), "NS_DF_SurveyAttributeMapping" },
            { typeof(SurveyAttributeValue), "NS_DF_SurveyAttributeValue" },
            { typeof(SurveySubmission), "NS_DF_SurveySubmission" },
            { typeof(PredefinedSurveyAttributeValue), "NS_DF_PredefinedSurveyAttributeValue" },
            { typeof(SurveySubmissionAttribute), "NS_DF_SurveySubmissionAttribute" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
