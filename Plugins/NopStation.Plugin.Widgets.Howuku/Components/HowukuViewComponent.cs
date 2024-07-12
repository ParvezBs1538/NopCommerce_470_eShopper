using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Howuku.Models;

namespace NopStation.Plugin.Widgets.Howuku.Components
{
    public class HowukuViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly HowukuSettings _howukuSettings;

        #endregion

        #region Ctor

        public HowukuViewComponent(HowukuSettings howukuSettings)
        {
            _howukuSettings = howukuSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_howukuSettings.EnablePlugin)
                return Content("");

            var model = new PublicHowukuModel()
            {
                ProjectKey = _howukuSettings.ProjectKey,
                SettingModeId = (int)_howukuSettings.SettingMode,
                Script = _howukuSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Howuku/Views/PublicInfo.cshtml", model);
        }
    }
}