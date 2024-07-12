using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Factories;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Components
{
    public class PWAFooterViewComponent : NopStationViewComponent
    {
        private readonly IProgressiveWebAppModelFactory _progressiveWebAppModelFactory;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        public PWAFooterViewComponent(IProgressiveWebAppModelFactory progressiveWebAppModelFactory,
            ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _progressiveWebAppModelFactory = progressiveWebAppModelFactory;
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = await _progressiveWebAppModelFactory.PrepareFooterComponentModelAsync();

            return View(model);
        }
    }
}