using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ZohoSalesIQ.Models;

namespace NopStation.Plugin.Widgets.ZohoSalesIQ.Components
{
    public class ZohoSalesIQViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly ZohoSalesIQSettings _zohoSalesIQSettings;

        #endregion

        #region Ctor

        public ZohoSalesIQViewComponent(ZohoSalesIQSettings zohoSalesIQSettings)
        {
            _zohoSalesIQSettings = zohoSalesIQSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_zohoSalesIQSettings.EnablePlugin)
                return Content("");

            var model = new PublicInfoModel()
            {
                Script = _zohoSalesIQSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.ZohoSalesIQ/Views/PublicInfo.cshtml", model);
        }
    }
}
