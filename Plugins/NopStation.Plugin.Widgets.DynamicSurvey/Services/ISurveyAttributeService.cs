using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial interface ISurveyAttributeService
    {
        #region Survey attributes

        Task DeleteSurveyAttributeAsync(SurveyAttribute surveyAttribute);

        Task DeleteSurveyAttributesAsync(IList<SurveyAttribute> surveyAttributes);

        Task<IPagedList<SurveyAttribute>> GetAllSurveyAttributesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<SurveyAttribute> GetSurveyAttributeByIdAsync(int surveyAttributeId);

        Task<IList<SurveyAttribute>> GetSurveyAttributeByIdsAsync(int[] surveyAttributeIds);

        Task InsertSurveyAttributeAsync(SurveyAttribute surveyAttribute);

        Task UpdateSurveyAttributeAsync(SurveyAttribute surveyAttribute);

        Task<int[]> GetNotExistingAttributesAsync(int[] attributeId);

        #endregion

        #region Survey attributes mappings

        Task DeleteSurveyAttributeMappingAsync(SurveyAttributeMapping surveyAttributeMapping);

        Task<IList<SurveyAttributeMapping>> GetSurveyAttributeMappingsBySurveyIdAsync(int surveyId);

        Task<SurveyAttributeMapping> GetSurveyAttributeMappingByIdAsync(int surveyAttributeMappingId);

        Task InsertSurveyAttributeMappingAsync(SurveyAttributeMapping surveyAttributeMapping);

        Task UpdateSurveyAttributeMappingAsync(SurveyAttributeMapping surveyAttributeMapping);

        #endregion

        #region Survey attribute values

        Task DeleteSurveyAttributeValueAsync(SurveyAttributeValue surveyAttributeValue);

        Task<IList<SurveyAttributeValue>> GetSurveyAttributeValuesAsync(int surveyAttributeMappingId);

        Task<SurveyAttributeValue> GetSurveyAttributeValueByIdAsync(int surveyAttributeValueId);

        Task InsertSurveyAttributeValueAsync(SurveyAttributeValue surveyAttributeValue);

        Task UpdateSurveyAttributeValueAsync(SurveyAttributeValue surveyAttributeValue);

        #endregion

        #region Predefined survey attribute values

        Task DeletePredefinedSurveyAttributeValueAsync(PredefinedSurveyAttributeValue psav);

        Task<IList<PredefinedSurveyAttributeValue>> GetPredefinedSurveyAttributeValuesAsync(int surveyAttributeId);

        Task<PredefinedSurveyAttributeValue> GetPredefinedSurveyAttributeValueByIdAsync(int id);

        Task InsertPredefinedSurveyAttributeValueAsync(PredefinedSurveyAttributeValue psav);

        Task UpdatePredefinedSurveyAttributeValueAsync(PredefinedSurveyAttributeValue psav);

        #endregion
    }
}
