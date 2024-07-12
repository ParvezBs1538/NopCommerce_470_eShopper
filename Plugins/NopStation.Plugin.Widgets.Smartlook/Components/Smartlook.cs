using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Smartlook.Models;

namespace NopStation.Plugin.Widgets.Smartlook.Components
{
    public class SmartlookViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly SmartlookSettings _smartlookSettings;

        #endregion

        #region Ctor

        public SmartlookViewComponent(SmartlookSettings smartlookSettings)
        {
            _smartlookSettings = smartlookSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_smartlookSettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                ProjectKey = _smartlookSettings.ProjectKey,
                SettingModeId = (int)_smartlookSettings.SettingMode,
                Script = _smartlookSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Smartlook/Views/PublicInfo.cshtml", model);
        }
    }
}
