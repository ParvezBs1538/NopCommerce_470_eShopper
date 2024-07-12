using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.LuckyOrange.Models;

namespace NopStation.Plugin.Widgets.LuckyOrange.Components
{
    public class LuckyOrangeViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly LuckyOrangeSettings _luckyOrangeSettings;

        #endregion

        #region Ctor

        public LuckyOrangeViewComponent(LuckyOrangeSettings luckyOrangeSettings)
        {
            _luckyOrangeSettings = luckyOrangeSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_luckyOrangeSettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                SiteId = _luckyOrangeSettings.SiteId,
                SettingModeId = (int)_luckyOrangeSettings.SettingMode,
                TrackingCode = _luckyOrangeSettings.TrackingCode
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.LuckyOrange/Views/PublicInfo.cshtml", model);
        }
    }
}
