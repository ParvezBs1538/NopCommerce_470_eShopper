using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace Nopstation.Plugin.Widgets.DooFinder.Components
{
    public class DooFinderScriptViewComponent : NopStationViewComponent
    {
        private readonly DooFinderSettings _dooFinderSettings;

        public DooFinderScriptViewComponent(DooFinderSettings dooFinderSettings)
        {
            _dooFinderSettings = dooFinderSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_dooFinderSettings.ActiveScript)
                return Content("");

            var script = _dooFinderSettings.ApiScript;
            return View("~/Plugins/NopStation.Plugin.Widgets.DooFinder/Views/ApiScript.cshtml", script);
        }
    }
}
