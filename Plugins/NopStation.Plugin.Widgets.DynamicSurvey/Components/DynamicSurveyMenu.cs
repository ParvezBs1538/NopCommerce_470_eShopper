using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DynamicSurvey.Factories;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Components
{
    public class DynamicSurveyMenuViewComponent : NopStationViewComponent
    {
        private readonly ISurveyModelFactory _surveyModelFactory;

        public DynamicSurveyMenuViewComponent(ISurveyModelFactory surveyModelFactory)
        {
            _surveyModelFactory = surveyModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = await _surveyModelFactory.PrepareSurveyTopMenuModelAsync(widgetZone);

            return View(model);
        }
    }
}
