using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.BulkDeleteCustomer.Components
{
    public partial class BulkDeleteCustomerViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/NopStation.Plugin.Widgets.BulkDeleteCustomer/Views/Component.cshtml");
        }
    }
}
