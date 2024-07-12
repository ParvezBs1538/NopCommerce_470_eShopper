using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories
{
    public partial interface ISurveyModelFactory
    {
        Task<SurveySearchModel> PrepareSurveySearchModelAsync(SurveySearchModel searchModel);

        Task<SurveyListModel> PrepareSurveyListModelAsync(SurveySearchModel searchModel);

        Task<SurveyModel> PrepareSurveyModelAsync(SurveyModel model, Survey survey, bool excludeProperties = false);

        Task<SurveyAttributeMappingListModel> PrepareSurveyAttributeMappingListModelAsync(SurveyAttributeMappingSearchModel searchModel,
            Survey survey);

        Task<SurveyAttributeMappingModel> PrepareSurveyAttributeMappingModelAsync(SurveyAttributeMappingModel model,
            Survey survey, SurveyAttributeMapping surveyAttributeMapping, bool excludeProperties = false);

        Task<SurveyAttributeValueListModel> PrepareSurveyAttributeValueListModelAsync(SurveyAttributeValueSearchModel searchModel,
            SurveyAttributeMapping surveyAttributeMapping);

        Task<SurveyAttributeValueModel> PrepareSurveyAttributeValueModelAsync(SurveyAttributeValueModel model,
            SurveyAttributeMapping surveyAttributeMapping, SurveyAttributeValue surveyAttributeValue, bool excludeProperties = false);

        Task<SurveySubmissionSearchModel> PrepareSurveySubmissionSearchModelAsync(SurveySubmissionSearchModel searchModel, Survey survey, bool excludeProperties = true);

        Task<SurveySubmissionListModel> PrepareSurveySubmissionListModelAsync(SurveySubmissionSearchModel searchModel);

        Task<SurveySubmissionModel> PrepareSurveySubmissionModelAsync(SurveySubmissionModel model, SurveySubmission surveySubmission, bool excludeProperties = true);
    }
}