using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Factories
{
    public partial interface ISurveyAttributeModelFactory
    {
        Task<SurveyAttributeSearchModel> PrepareSurveyAttributeSearchModelAsync(SurveyAttributeSearchModel searchModel);

        Task<SurveyAttributeListModel> PrepareSurveyAttributeListModelAsync(SurveyAttributeSearchModel searchModel);

        Task<SurveyAttributeModel> PrepareSurveyAttributeModelAsync(SurveyAttributeModel model,
            SurveyAttribute surveyAttribute, bool excludeProperties = false);

        Task<PredefinedSurveyAttributeValueListModel> PreparePredefinedSurveyAttributeValueListModelAsync(
           PredefinedSurveyAttributeValueSearchModel searchModel, SurveyAttribute surveyAttribute);

        Task<PredefinedSurveyAttributeValueModel> PreparePredefinedSurveyAttributeValueModelAsync(PredefinedSurveyAttributeValueModel model,
            SurveyAttribute surveyAttribute, PredefinedSurveyAttributeValue surveyAttributeValue, bool excludeProperties = false);

        Task<SurveyAttributeSurveyListModel> PrepareSurveyAttributeSurveyListModelAsync(SurveyAttributeSurveySearchModel searchModel,
            SurveyAttribute surveyAttribute);
    }
}