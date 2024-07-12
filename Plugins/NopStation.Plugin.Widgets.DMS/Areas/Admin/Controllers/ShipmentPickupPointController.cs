using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipmentPickupPoint;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Controllers
{
    public class ShipmentPickupPointController : NopStationAdminController
    {
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IShipmentPickupPointModelFactory _shipmentPickupPointModelFactory;
        private readonly IShipmentPickupPointService _shipmentPickupPointService;
        #region Fields



        #endregion

        #region Ctor

        public ShipmentPickupPointController(
            IAddressService addressService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IShipmentPickupPointModelFactory shipmentPickupPointModelFactory,
            IShipmentPickupPointService shipmentPickupPointService
            )
        {
            _addressService = addressService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _shipmentPickupPointModelFactory = shipmentPickupPointModelFactory;
            _shipmentPickupPointService = shipmentPickupPointService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods
        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipmentPickupPoint))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipmentPickupPoint))
                return AccessDeniedView();

            var model = _shipmentPickupPointModelFactory.PrepareShipmentPickupPointSearchModel(new ShipmentPickupPointSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ShipmentPickupPointSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _shipmentPickupPointModelFactory.PrepareShipmentPickupPointListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = await _shipmentPickupPointModelFactory.PrepareShipmentPickupPointModelAsync(new ShipmentPickupPointModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Create(ShipmentPickupPointModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                var address = new Address
                {
                    Address1 = model.Address.Address1,
                    City = model.Address.City,
                    County = model.Address.County,
                    CountryId = model.Address.CountryId,
                    StateProvinceId = model.Address.StateProvinceId,
                    ZipPostalCode = model.Address.ZipPostalCode,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _addressService.InsertAddressAsync(address);

                var pickupPoint = new ShipmentPickupPoint
                {
                    Name = model.Name,
                    Description = model.Description,
                    AddressId = address.Id,
                    OpeningHours = model.OpeningHours,
                    DisplayOrder = model.DisplayOrder,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                };
                await _shipmentPickupPointService.InsertShipmentPickupPointAsync(pickupPoint);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DMS.ShipmentPickupPoint.Create.Successful"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = pickupPoint.Id });
            }
            model = await _shipmentPickupPointModelFactory.PrepareShipmentPickupPointModelAsync(model, null);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DMS.ShipmentPickupPoint.Create.Failed"));
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var pickupPoint = await _shipmentPickupPointService.GetShipmentPickupPointByIdAsync(id);
            if (pickupPoint == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DMS.ShipmentPickupPoint.PickupPointNotFound"));
                return RedirectToAction("List");
            }

            var model = await _shipmentPickupPointModelFactory.PrepareShipmentPickupPointModelAsync(new ShipmentPickupPointModel(), pickupPoint);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(ShipmentPickupPointModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Edit(model.Id);

            var pickupPoint = await _shipmentPickupPointService.GetShipmentPickupPointByIdAsync(model.Id);
            if (pickupPoint == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DMS.ShipmentPickupPoint.PickupPointNotFound"));
                return RedirectToAction("List");
            }

            var address = await _addressService.GetAddressByIdAsync(pickupPoint.AddressId) ?? new Address { CreatedOnUtc = DateTime.UtcNow };
            address.Address1 = model.Address.Address1;
            address.City = model.Address.City;
            address.County = model.Address.County;
            address.CountryId = model.Address.CountryId;
            address.StateProvinceId = model.Address.StateProvinceId;
            address.ZipPostalCode = model.Address.ZipPostalCode;
            if (address.Id > 0)
                await _addressService.UpdateAddressAsync(address);
            else
                await _addressService.InsertAddressAsync(address);

            pickupPoint.Name = model.Name;
            pickupPoint.Description = model.Description;
            pickupPoint.AddressId = address.Id;
            pickupPoint.OpeningHours = model.OpeningHours;
            pickupPoint.DisplayOrder = model.DisplayOrder;
            pickupPoint.Latitude = model.Latitude;
            pickupPoint.Longitude = model.Longitude;
            await _shipmentPickupPointService.UpdateShipmentPickupPointAsync(pickupPoint);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DMS.ShipmentPickupPoint.Edit.Successful"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = pickupPoint.Id });
        }

        #endregion
    }
}
