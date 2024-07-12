using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Factories;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Controllers
{
    public class DMSController : NopStationPublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IShipperService _shipperService;
        private readonly ILocalizationService _localizationService;
        private readonly ICourierShipmentModelFactory _courierShipmentModelFactory;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly DMSSettings _dMSSettings;
        private readonly IOrderModelFactory _orderModelFactory;

        #endregion

        #region Ctor

        public DMSController(IWorkContext workContext,
            IOrderModelFactory orderModelFactory,
            ICustomerService customerService,
            IShipperService shipperService,
            ILocalizationService localizationService,
            ICourierShipmentModelFactory courierShipmentModelFactory,
            ICourierShipmentService courierShipmentService,
            IShipmentService shipmentService,
            IOrderProcessingService orderProcessingService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            DMSSettings dMSSettings
            )
        {
            _workContext = workContext;
            _orderModelFactory = orderModelFactory;
            _customerService = customerService;
            _shipperService = shipperService;
            _localizationService = localizationService;
            _courierShipmentModelFactory = courierShipmentModelFactory;
            _courierShipmentService = courierShipmentService;
            _shipmentService = shipmentService;
            _orderProcessingService = orderProcessingService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _dMSSettings = dMSSettings;


        }

        #endregion

        #region Methods

        public async Task<IActionResult> Shipments(CourierShipmentsModel command, ShipmentSearchModel searchModel)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            var model = new CourierShipmentsModel();

            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            if (shipper == null || !shipper.Active)
            {
                model.WarningMessage = shipper == null ?
                    await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.InvalidAccount") :
                    await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.InactiveAccount");
                model.InvalidAccount = true;

                return View(model);
            }

            model.UseAjaxLoading = _dMSSettings.UseAjaxLoading;
            model = await _courierShipmentModelFactory.PrepareCourierShipmentsOverviewModelAsync(model, shipper, command, searchModel);
            return View(model);
        }

        public async Task<IActionResult> ShipmentsPartial(CourierShipmentsModel command, ShipmentSearchModel searchModel)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            var model = new CourierShipmentsModel();

            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            if (shipper == null || !shipper.Active)
            {
                model.WarningMessage = shipper == null ?
                    await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.InvalidAccount") :
                    await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.InactiveAccount");
                model.InvalidAccount = true;

                return View(model);
            }

            model.UseAjaxLoading = _dMSSettings.UseAjaxLoading;
            model = await _courierShipmentModelFactory.PrepareCourierShipmentsOverviewModelAsync(model, shipper, command, searchModel);

            return PartialView("_Shipments", model);
        }

        public async Task<IActionResult> ShipmentDetails(int shipmentId)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId); // **
            if (shipment == null)
                return RedirectToRoute("DMSShipmentDetails");

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Challenge();

            var model = await _courierShipmentModelFactory.PrepareShipmentDetailsModelAsync(shipment, courierShipment);
            return View(model);
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public async Task<IActionResult> MarkAsShipped(int shipmentId)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return RedirectToRoute("DMSShipmentDetails");

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Challenge();

            try
            {
                await _orderProcessingService.ShipAsync(shipment, true);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsShipped"));
            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToRoute("DMSShipmentDetails", new { shipmentId = shipmentId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("btnEditShippedDate")]
        public virtual async Task<IActionResult> EditShippedDate(CourierShipmentDetailsModel model)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);
            if (shipment == null)
                return RedirectToRoute("DMSShipmentDetails");

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Challenge();

            try
            {
                if (!model.ShippedDate.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }

                shipment.ShippedDateUtc = model.ShippedDate;
                await _shipmentService.UpdateShipmentAsync(shipment);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsShippedDateUpdate"));

            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
                //return RedirectToAction("DMSShipmentDetails", new { shipmentId = model.ShipmentId });
            }
            return RedirectToRoute("DMSShipmentDetails", new { shipmentId = model.ShipmentId });
        }

        [HttpPost]
        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        public async Task<IActionResult> MarkAsDelivered(int shipmentId)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return RedirectToRoute("DMSShipmentDetails");

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Challenge();

            try
            {
                await _orderProcessingService.DeliverAsync(shipment, true);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsDelivered"));
            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToRoute("DMSShipmentDetails", new { shipmentId = shipmentId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("btnEditDeliveryDate")]
        public virtual async Task<IActionResult> EditDeliveredDate(CourierShipmentDetailsModel model)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Challenge();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.ShipmentId);
            if (shipment == null)
                return RedirectToRoute("DMSShipmentDetails");

            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(model.ShipmentId);
            var shipper = await _shipperService.GetShipperByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            if (courierShipment == null || courierShipment.ShipperId != shipper.Id)
                return Challenge();

            try
            {
                if (!model.DeliveryDate.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }

                shipment.DeliveryDateUtc = model.DeliveryDate;
                await _shipmentService.UpdateShipmentAsync(shipment);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.MarkedAsDeliveryDateUpdate"));

            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
                //return RedirectToAction("DMSShipmentDetails", new { shipmentId = model.ShipmentId });
            }
            return RedirectToRoute("DMSShipmentDetails", new { shipmentId = model.ShipmentId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkCourierShipmentAsRecieved(int shipmentId)
        {
            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);

            if (courierShipment == null)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.CourierShipment.NotFound") });


            var customer = await _workContext.GetCurrentCustomerAsync();

            if (customer == null || !customer.Active || customer.Deleted)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound") });

            if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.ReceivedByShipper && courierShipment.ShipperId == customer.Id)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceived") });

            var shipment = await _shipmentService.GetShipmentByIdAsync(courierShipment.ShipmentId);

            if (shipment == null)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.Shipment.NotFound") });

            if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.AssignedToShipper)
            {
                if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.ReceivedByShipper)
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceivedByShipper") });
                else
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShippingNotPossible") });
            }

            var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

            if (shipper == null)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound") });

            if (customer.Id != shipper.CustomerId)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.WrongShipper") });

            courierShipment.ShipmentStatusType = ShipmentStatusTypes.ReceivedByShipper;

            await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);

            if (!shipment.ShippedDateUtc.HasValue)
                await _orderProcessingService.ShipAsync(shipment, true);

            return Json(new { success = true, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.MarkedAsReceived") });
        }


        [HttpPost]
        public async Task<IActionResult> MarkCourierShipmentAsDelivered(int shipmentId)
        {
            var courierShipment = await _courierShipmentService.GetCourierShipmentByShipmentIdAsync(shipmentId);

            if (courierShipment == null)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.CourierShipment.NotFound") });


            var customer = await _workContext.GetCurrentCustomerAsync();

            if (customer == null || !customer.Active || customer.Deleted)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound") });

            if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.ReceivedByShipper && courierShipment.ShipperId == customer.Id)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceived") });

            var shipment = await _shipmentService.GetShipmentByIdAsync(courierShipment.ShipmentId);

            if (shipment == null)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.Shipment.NotFound") });

            if (courierShipment.ShipmentStatusType != ShipmentStatusTypes.AssignedToShipper)
            {
                if (courierShipment.ShipmentStatusType == ShipmentStatusTypes.ReceivedByShipper)
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceivedByShipper") });
                else
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShippingNotPossible") });
            }

            var shipper = await _shipperService.GetShipperByIdAsync(courierShipment.ShipperId);

            if (shipper == null)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound") });

            if (customer.Id != shipper.CustomerId)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.WrongShipper") });

            courierShipment.ShipmentStatusType = ShipmentStatusTypes.ReceivedByShipper;

            await _courierShipmentService.UpdateCourierShipmentAsync(courierShipment);

            if (!shipment.ShippedDateUtc.HasValue)
                await _orderProcessingService.ShipAsync(shipment, true);

            return Json(new { success = true, message = await _localizationService.GetResourceAsync("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.MarkedAsReceived") });
        }

        #endregion
    }
}
