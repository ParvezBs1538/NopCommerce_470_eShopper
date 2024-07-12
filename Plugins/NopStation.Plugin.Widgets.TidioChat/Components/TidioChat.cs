using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.TidioChat.Models;

namespace NopStation.Plugin.Widgets.TidioChat.Components
{
    public class TidioChatViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly TidioChatSettings _tidioChatSettings;

        #endregion

        #region Ctor

        public TidioChatViewComponent(TidioChatSettings tidioChatSettings)
        {
            _tidioChatSettings = tidioChatSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_tidioChatSettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                Script = _tidioChatSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.TidioChat/Views/PublicInfo.cshtml", model);
        }
    }
}
