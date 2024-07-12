using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;
using NopStation.Plugin.Widgets.DynamicSurvey.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Factories
{
    public interface ISurveyModelFactory
    {
        Task<SurveyTopMenuModel> PrepareSurveyTopMenuModelAsync(string widgetZone);

        Task<IList<SurveyOverviewModel>> PrepareSurveyOverviewModelsAsync(string widgetZone);

        Task<SurveyModel> PrepareSurveyModelBySystemNameAsync(string systemName);

        Task<SurveyModel> PrepareSurveyModelByIdAsync(Survey survey, string attributesXml = "");
    }
}