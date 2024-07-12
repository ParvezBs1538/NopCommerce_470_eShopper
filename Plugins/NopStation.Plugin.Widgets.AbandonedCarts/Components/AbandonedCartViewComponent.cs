using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.AbandonedCarts.Factories;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Components
{
    public class AbandonedCartViewComponent : NopStationViewComponent
    {
        private readonly IAbandonedCartFactory _abandonedCartFactory;

        public AbandonedCartViewComponent(IAbandonedCartFactory abandonedCartFactory)
        {
            _abandonedCartFactory = abandonedCartFactory;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (widgetZone == AdminWidgetZones.MaintenanceDetailsBlock)
            {
                return View("~/Plugins/NopStation.Plugin.Widgets.AbandonedCarts/Views/AbandonedCart/MaintananceDeleteAbandonedCarts.cshtml",
                    new AbandonmentMaintenanceModel());
            }
            else if (additionalData is CustomerModel { Id: > 0 } model)
            {
                var returnModel = _abandonedCartFactory.PrepareAbandonedCartSearchModel(new AbandonedCartSearchModel() { CustomerId = model.Id });
                return View("~/Plugins/NopStation.Plugin.Widgets.AbandonedCarts/Views/AbandonedCart/AbandonmentCustomerDetails.cshtml", returnModel);
            }
            return Content("");
        }
    }
}