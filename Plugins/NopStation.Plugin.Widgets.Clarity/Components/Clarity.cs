using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Clarity.Models;

namespace NopStation.Plugin.Widgets.Clarity.Components
{
    public class ClarityViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly ClaritySettings _claritySettings;

        #endregion

        #region Ctor

        public ClarityViewComponent(ClaritySettings claritySettings)
        {
            _claritySettings = claritySettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_claritySettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                ProjectId = _claritySettings.ProjectId,
                SettingModeId = (int)_claritySettings.SettingMode,
                TrackingCode = _claritySettings.TrackingCode
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Clarity/Views/PublicInfo.cshtml", model);
        }
    }
}
