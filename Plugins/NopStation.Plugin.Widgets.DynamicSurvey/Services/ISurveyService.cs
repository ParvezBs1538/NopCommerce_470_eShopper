using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public interface ISurveyService
    {
        Task DeleteSurveyAsync(Survey survey);

        Task DeleteSurveysAsync(IList<Survey> surveys);

        Task InsertSurveyAsync(Survey survey);

        Task UpdateSurveyAsync(Survey survey);

        Task<Survey> GetSurveyByIdAsync(int surveyId);

        Task<IList<Survey>> GetSurveysByIdsAsync(int[] surveyIds);

        Task<IList<Survey>> SearchSurveysAsync(string keywords = null, int storeId = 0, bool? includedInTopMenu = null,
            bool showHidden = false, bool? overridePublished = null, string widgetZone = null, bool validScheduleOnly = false);

        Task<Survey> GetSurveyBySystemNameAsync(string systemName, int storeId = 0, bool showHidden = false);

        Task UpdateSurveyStoreMappingsAsync(Survey survey, IList<int> limitedToStoresIds);

        Task<IPagedList<Survey>> GetSurveysBySurveyAttributeIdAsync(int surveyAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task DeleteSurveySubmissionAsync(SurveySubmission surveySubmission);

        Task DeleteSurveySubmissionsAsync(IList<SurveySubmission> surveySubmissions);

        Task InsertSurveySubmissionAsync(SurveySubmission surveySubmission);

        Task UpdateSurveySubmissionAsync(SurveySubmission surveySubmission);

        Task<SurveySubmission> GetSurveySubmissionByIdAsync(int surveySubmissionId);

        Task<IList<SurveySubmission>> GetSurveySubmissionsBySurveyIdAsync(int surveyId, int customerId = 0, bool loadOnlyOne = false);

        #region Survey submission attributes

        Task DeleteSurveySubmissionAttributeAsync(SurveySubmissionAttribute surveySubmissionAttribute);

        Task DeleteSurveySubmissionAttributesAsync(IList<SurveySubmissionAttribute> surveySubmissionAttributes);

        Task InsertSurveySubmissionAttributeAsync(SurveySubmissionAttribute surveySubmissionAttribute);

        Task UpdateSurveySubmissionAttributeAsync(SurveySubmissionAttribute surveySubmissionAttribute);

        Task<SurveySubmissionAttribute> GetSurveySubmissionAttributeByIdAsync(int surveySubmissionAttributeId);

        Task<IList<SurveySubmissionAttribute>> GetSurveySubmissionAttributesBySurveySubmissionIdAsync(int surveySubmissionId);
        Task<byte[]> ExportSurveysToXlsxAsync(IList<SurveySubmission> surveySubmissions);
        Task<IPagedList<SurveySubmission>> GetAllSurveySubmissionsAsync(int surveyId = 0, int customerId = 0, string customerEmail = null, DateTime? dateStart = null, DateTime? dateEnd = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IList<SurveySubmission>> GetSurveySubmissionsByIdsAsync(int[] surveySubmissionIds);

        #endregion
    }
}