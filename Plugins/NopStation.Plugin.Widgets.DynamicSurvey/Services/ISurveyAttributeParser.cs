using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial interface ISurveyAttributeParser
    {
        #region Survey attributes

        Task<IList<SurveyAttributeMapping>> ParseSurveyAttributeMappingsAsync(string attributesXml);

        Task<IList<SurveyAttributeValue>> ParseSurveyAttributeValuesAsync(string attributesXml, int surveyAttributeMappingId = 0);

        IList<string> ParseValues(string attributesXml, int surveyAttributeMappingId);

        string AddSurveyAttribute(string attributesXml, SurveyAttributeMapping surveyAttributeMapping, string value);

        string RemoveSurveyAttribute(string attributesXml, SurveyAttributeMapping surveyAttributeMapping);

        Task<bool> AreSurveyAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes);

        Task<bool?> IsConditionMetAsync(SurveyAttributeMapping pam, string selectedAttributesXml);

        Task<(string AttributesXml, AttributeMetadata MetaData)> ParseSurveyAttributesAsync(Survey survey, IFormCollection form);

        Task<IList<string>> GetSurveyAttributeWarningsAsync(Survey survey, string attributesXml = "",
            bool ignoreNonCombinableAttributes = false, bool ignoreConditionMet = false);

        #endregion
    }
}
