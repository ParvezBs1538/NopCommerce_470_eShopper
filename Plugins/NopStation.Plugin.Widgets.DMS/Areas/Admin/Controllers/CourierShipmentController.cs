using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Controllers
{
    public class CourierShipmentController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IShipmentService _shipmentService;
        private readonly IShipperService _shipperService;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly ICourierShipmentModelFactory _courierShipmentModelFactory;
        private readonly IShipmentNoteService _shipmentNoteService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CourierShipmentController(ILocalizationService localizationService,
            IPermissionService permissionService,
            IShipmentService shipmentService,
            IShipperService shipperService,
            ICourierShipmentService courierShipmentService,
            ICourierShipmentModelFactory courierShipmentModelFactory,
            IShipmentNoteService shipmentNoteService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IWorkContext workContext
            )
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _shipmentService = shipmentService;
            _shipperService = shipperService;
            _courierShipmentService = courierShipmentService;
            _courierShipmentModelFactory = courierShipmentModelFactory;
            _shipmentNoteService = shipmentNoteService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageCourierShipment))
                return AccessDeniedView();

            var model = await _courierShipmentModelFactory.PrepareCourierShipmentSearchModelAsync(new CourierShipmentSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(CourierShipmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageCourierShipment))
                return await AccessDeniedDataTablesJson();

            var model = await _courierShipmentModelFactory.PrepareCourierShipmentListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> SaveCourierShipment(CourierShipmentModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) ||
                !await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageCourierShipment))
                return await AccessDeniedDataTablesJson();

            var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId)
                    ?? throw new ArgumentException("No shipment found with this specific id.");

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId)
                    ?? throw new ArgumentException("No order found with this specific id.");

            if (order.PickupInStore)
                throw new ArgumentException("Order is pickup in store type. No courier shipment required");

            if (shipment.DeliveryDateUtc.HasValue)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.Shipment.Delivered"));

            var shipper = await _shipperService.GetShipperByIdAsync(model.ShipperId);
            if (shipper == null)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.Fields.Shipper.NotFound"));

            if (!ModelState.IsValid)
            {
                var errorList = "";
                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
                        errorList += error.ErrorMessage + Environment.NewLine;

                return Json(new { Message = errorList, Result = false });
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (customer == null)
                throw new Exception(nameof(customer));

            try
            {
                //var shipper = await _shipperService.GetShipperByIdAsync(model.ShipperId);
                if (shipper != null && !shipment.DeliveryDateUtc.HasValue)
                {
                    var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipment.Id);

                    if (courierShipment == null)
                    {
                        courierShipment = new CourierShipment
                        {
                            ShipmentId = shipment.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow,
                            ShipperId = shipper.Id,
                            ShipmentPickupPointId = model.ShipmentPickupPointId,
                            //SignaturePictureId = model.SignaturePictureId,
                            ShipmentStatusId = (int)ShipmentStatusTypes.AssignedToShipper
                        };
                        await _courierShipmentService.InsertCourierShipmentAsync(courierShipment);

                        var note = $"Courier Shipment for shipment Id#{shipment.Id}";
                        var shipmentNote = new ShipmentNote()
                        {
                            CourierShipmentId = courierShipment.Id,
                            NopShipmentId = shipment.Id,
                            Note = note,
                            DisplayToShipper = true,
                            DisplayToCustomer = true,
                            UpdatedByCustomerId = customer.Id
                        };
                        await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);
                    }
                    else
                    {
                        bool valueUpdated = false;

                        if (courierShipment.ShipmentStatusId != model.ShipmentStatusId)
                        {
                            courierShipment.ShipmentStatusId = model.ShipmentStatusId;
                            valueUpdated = true;
                            var note = $"Courier Shipment status updated for shipment Id#{shipment.Id}. Current status: {(ShipmentStatusTypes)model.ShipmentStatusId}";
                            var shipmentNote = new ShipmentNote()
                            {
                                CourierShipmentId = courierShipment.Id,
                                NopShipmentId = shipment.Id,
                                Note = note,
                                DisplayToShipper = false,
                                DisplayToCustomer = true,
                                UpdatedByCustomerId = customer.Id
                            };

                            if (model.ShipmentStatusId == (int)ShipmentStatusTypes.AssignedToShipper)
                            {
                                if (shipment.ShippedDateUtc.HasValue)
                                {
                                    shipment.ShippedDateUtc = null;
                                    await _shipmentService.UpdateShipmentAsync(shipment);
                                }
                                shipmentNote.DisplayToCustomer = false;
                            }
                            else if (model.ShipmentStatusId == (int)ShipmentStatusTypes.ReceivedByShipper)
                            {
                                if (!shipment.ShippedDateUtc.HasValue)
                                    await _orderProcessingService.ShipAsync(shipment, false);
                                shipmentNote.DisplayToShipper = true;
                            }
                            await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);
                        }
                        if (courierShipment.ShipperId != model.ShipperId)
                        {
                            courierShipment.ShipperId = shipper.Id;
                            valueUpdated = true;
                            var note = $"Courier Shipment Shipper updated for shipment Id#{shipment.Id}";
                            var shipmentNote = new ShipmentNote()
                            {
                                CourierShipmentId = courierShipment.Id,
                                NopShipmentId = shipment.Id,
                                Note = note,
                                DisplayToShipper = false,
                                DisplayToCustomer = true,
                                UpdatedByCustomerId = customer.Id
                            };

                            await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);
                        }

                        if (courierShipment.ShipmentPickupPointId != model.ShipmentPickupPointId)
                        {
                            courierShipment.ShipmentPickupPointId = model.ShipmentPickupPointId;
                            valueUpdated = true;
                            var note = $"Courier pickup point updated for shipment Id#{shipment.Id}";
                            var shipmentNote = new ShipmentNote()
                            {
                                CourierShipmentId = courierShipment.Id,
                                NopShipmentId = shipment.Id,
                                Note = note,
                                DisplayToShipper = false,
                                DisplayToCustomer = true,
                                UpdatedByCustomerId = customer.Id
                            };

                            await _shipmentNoteService.InsertShipmentNoteAsync(shipmentNote);
                        }
                        if (valueUpdated)
                        {
                            courierShipment.UpdatedOnUtc = DateTime.UtcNow;
                            await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);
                        }
                    }

                    model = await _courierShipmentModelFactory.PrepareCourierShipmentModelAsync(null, courierShipment, shipment);
                    var validHtml = await RenderPartialViewToStringAsync("Components/CourierShipment/Default", model);

                    return Json(new { Html = validHtml, Result = true });
                }

                if (shipper == null)
                    ModelState.AddModelError("ShipperId", await _localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.Fields.Shipper.NotFound"));
                else
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.Shipment.Delivered"));

                model = await _courierShipmentModelFactory.PrepareCourierShipmentModelAsync(model, null, shipment);
                var invalidHtml = await RenderPartialViewToStringAsync("Components/CourierShipment/Default", model);

                return Json(new { Html = invalidHtml, Result = true });
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Result = false });
            }
        }

        #endregion
    }
}
