using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DynamicSurvey.Factories;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Components
{
    public class DynamicSurveyBlockViewComponent : NopStationViewComponent
    {
        private readonly ISurveyModelFactory _surveyModelFactory;

        public DynamicSurveyBlockViewComponent(ISurveyModelFactory surveyModelFactory)
        {
            _surveyModelFactory = surveyModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string systemName)
        {
            var model = await _surveyModelFactory.PrepareSurveyModelBySystemNameAsync(systemName);
            if (model == null)
                return Content("");

            return View(model);
        }
    }
}
