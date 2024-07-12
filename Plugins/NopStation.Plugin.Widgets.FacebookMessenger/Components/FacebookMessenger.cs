using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.FacebookMessenger.Models;
using Nop.Services.Configuration;

namespace NopStation.Plugin.Widgets.FacebookMessenger.Components
{
    public class FacebookMessengerViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly FacebookMessengerSettings _facebookMessengerSettings;

        #endregion

        #region Ctor

        public FacebookMessengerViewComponent(ISettingService settingService,
            IStoreContext storeContext,
            FacebookMessengerSettings facebookMessengerSettings)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _facebookMessengerSettings = facebookMessengerSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_facebookMessengerSettings.EnablePlugin)
                return Content("");

            var publicModel = new PublicInfoModel()
            {
                PageId = _facebookMessengerSettings.PageId,
                ThemeColor = _facebookMessengerSettings.ThemeColor,
                EnableScript = _facebookMessengerSettings.EnableScript,
                Script = _facebookMessengerSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.FacebookMessenger/Views/PublicInfo.cshtml", publicModel);
        }
    }
}
