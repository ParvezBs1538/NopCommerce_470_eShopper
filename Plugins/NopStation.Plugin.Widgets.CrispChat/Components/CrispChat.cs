using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.CrispChat.Models;

namespace NopStation.Plugin.Widgets.CrispChat.Components
{
    public class CrispChatViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly CrispChatSettings _crispChatSettings;

        #endregion

        #region Ctor

        public CrispChatViewComponent(CrispChatSettings crispChatSettings)
        {
            _crispChatSettings = crispChatSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_crispChatSettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                WebsiteId = _crispChatSettings.WebsiteId,
                SettingModeId = (int)_crispChatSettings.SettingMode,
                Script = _crispChatSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.CrispChat/Views/PublicInfo.cshtml", model);
        }
    }
}
