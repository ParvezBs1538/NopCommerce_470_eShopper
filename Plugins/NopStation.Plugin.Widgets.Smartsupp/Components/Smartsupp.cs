using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Smartsupp.Models;

namespace NopStation.Plugin.Widgets.Smartsupp.Components
{
    public class SmartsuppViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly SmartsuppSettings _smartsuppSettings;

        #endregion

        #region Ctor

        public SmartsuppViewComponent(SmartsuppSettings smartsuppSettings)
        {
            _smartsuppSettings = smartsuppSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_smartsuppSettings.EnablePlugin)
                return Content("");

            var model = new PublicSmartsuppModel()
            {
                Key = _smartsuppSettings.Key,
                SettingModeId = (int)_smartsuppSettings.SettingMode,
                Script = _smartsuppSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Smartsupp/Views/PublicInfo.cshtml", model);
        }
    }
}
