using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Mouseflow.Models;

namespace NopStation.Plugin.Widgets.Mouseflow.Components
{
    public class MouseflowViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly MouseflowSettings _mouseflowSettings;

        #endregion

        #region Ctor

        public MouseflowViewComponent(MouseflowSettings mouseflowSettings)
        {
            _mouseflowSettings = mouseflowSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_mouseflowSettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                WebsiteId = _mouseflowSettings.WebsiteId,
                SettingModeId = (int)_mouseflowSettings.SettingMode,
                Script = _mouseflowSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Mouseflow/Views/PublicInfo.cshtml", model);
        }
    }
}
