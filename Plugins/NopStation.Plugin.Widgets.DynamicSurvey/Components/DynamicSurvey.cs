using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DynamicSurvey.Factories;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Components
{
    public class DynamicSurveyViewComponent : NopStationViewComponent
    {
        private readonly ISurveyModelFactory _surveyModelFactory;

        public DynamicSurveyViewComponent(ISurveyModelFactory surveyModelFactory)
        {
            _surveyModelFactory = surveyModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = await _surveyModelFactory.PrepareSurveyOverviewModelsAsync(widgetZone);
            return View(model);
        }
    }
}
