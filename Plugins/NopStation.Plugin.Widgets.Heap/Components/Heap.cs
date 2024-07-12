using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Heap.Models;

namespace NopStation.Plugin.Widgets.Heap.Components
{
    public class HeapViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly HeapSettings _heapSettings;

        #endregion

        #region Ctor

        public HeapViewComponent(HeapSettings heapSettings)
        {
            _heapSettings = heapSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_heapSettings.EnablePlugin)
                return Content("");

            var model = new PublicHeapModel()
            {
                AppId = _heapSettings.AppId,
                SettingModeId = (int)_heapSettings.SettingMode,
                Script = _heapSettings.Script
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.Heap/Views/PublicInfo.cshtml", model);
        }
    }
}
