using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.AllInOneContactUs.Models;

namespace NopStation.Plugin.Widgets.AllInOneContactUs.Components
{
    public class WidgetsARContactUsViewComponent : NopStationViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = await Task.FromResult(new ARContactUsPublicModel());

            return View("~/Plugins/NopStation.Plugin.Widgets.AllInOneContactUs/Views/WidgetsARContatUs/PublicInfo.cshtml", model);
        }
    }
}
