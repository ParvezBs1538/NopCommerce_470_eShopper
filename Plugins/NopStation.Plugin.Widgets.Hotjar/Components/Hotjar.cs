using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Hotjar.Models;

namespace NopStation.Plugin.Widgets.Hotjar.Components
{
    public class HotjarViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly HotjarSettings _hotjarSettings;

        #endregion

        #region Ctor

        public HotjarViewComponent(HotjarSettings hotjarSettings)
        {
            _hotjarSettings = hotjarSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_hotjarSettings.EnablePlugin)
                return Content("");

            var model = new PublicHotjarModel()
            {
                SiteId = _hotjarSettings.SiteId,
                SettingModeId = (int)_hotjarSettings.SettingMode,
                Script = _hotjarSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Hotjar/Views/PublicInfo.cshtml", model);
        }
    }
}
