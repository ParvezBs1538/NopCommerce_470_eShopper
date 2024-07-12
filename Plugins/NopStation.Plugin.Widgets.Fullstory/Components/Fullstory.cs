using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Fullstory.Models;

namespace NopStation.Plugin.Widgets.Fullstory.Components
{
    public class FullstoryViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly FullstorySettings _fullstorySettings;

        #endregion

        #region Ctor

        public FullstoryViewComponent(FullstorySettings fullstorySettings)
        {
            _fullstorySettings = fullstorySettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_fullstorySettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                OrganizationId = _fullstorySettings.OrganizationId,
                SettingModeId = (int)_fullstorySettings.SettingMode,
                Script = _fullstorySettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Fullstory/Views/PublicInfo.cshtml", model);
        }
    }
}
