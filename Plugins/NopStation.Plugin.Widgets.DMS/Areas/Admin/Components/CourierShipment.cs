using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Components
{
    public class CourierShipmentViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IShipmentService _shipmentService;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly ICourierShipmentModelFactory _courierShipmentModelFactory;

        #endregion

        #region Ctor

        public CourierShipmentViewComponent(IShipmentService shipmentService,
            ICourierShipmentService courierShipmentService,
            ICourierShipmentModelFactory courierShipmentModelFactory)
        {
            _shipmentService = shipmentService;
            _courierShipmentService = courierShipmentService;
            _courierShipmentModelFactory = courierShipmentModelFactory;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (typeof(ShipmentModel) != additionalData.GetType())
                return Content("");

            var sm = additionalData as ShipmentModel;
            if (sm.PickupInStore)
                return Content("");

            var shipment = await _shipmentService.GetShipmentByIdAsync(sm.Id);
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(sm.Id);
            var model = await _courierShipmentModelFactory.PrepareCourierShipmentModelAsync(null, courierShipment, shipment);

            return View(model);
        }
    }
}
