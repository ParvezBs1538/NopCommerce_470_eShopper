using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Services;
namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Controller
{
    public partial class ShipperController : NopStationAdminController
    {
        #region Fields

        private readonly IShipperService _shipperService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly IShipperModelFactory _shipperModelFactory;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IShipperDeviceService _shipperDeviceService;
        private readonly ICourierShipmentModelFactory _courierShipmentModelFactory;

        #endregion

        #region Ctor

        public ShipperController(IShipperService shipperService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPermissionService permissionService,
            IShipperModelFactory shipperModelFactory,
            ICustomerModelFactory customerModelFactory,
            ICustomerService customerService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IShipperDeviceService shipperDeviceService,
            ICourierShipmentModelFactory courierShipmentModelFactory)
        {
            _shipperService = shipperService;
            _settingService = settingService;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _shipperModelFactory = shipperModelFactory;
            _customerModelFactory = customerModelFactory;
            _customerService = customerService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _shipperDeviceService = shipperDeviceService;
            _courierShipmentModelFactory = courierShipmentModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return AccessDeniedView();

            var model = _shipperModelFactory.PrepareShipperSearchModel(new ShipperSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ShipperSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return await AccessDeniedDataTablesJson();

            var model = await _shipperModelFactory.PrepareShipperListModelAsync(searchModel);

            return Json(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return AccessDeniedView();

            var shipper = await _shipperService.GetShipperByIdAsync(id)
                ?? throw new ArgumentException("No shipper found with the specified id");

            var model = await _shipperModelFactory.PrepareShipperModelAsync(new ShipperModel(), shipper);

            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ShipperModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var shipper = await _shipperService.GetShipperByIdAsync(model.Id);
            if (shipper == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                shipper.Active = model.Active;
                await _shipperService.UpdateShipperAsync(shipper);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = shipper.Id });
            }

            model = await _shipperModelFactory.PrepareShipperModelAsync(model, shipper, true);
            return View(model);
        }
        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> Delete(ShipperModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return await AccessDeniedDataTablesJson();

            var shipper = await _shipperService.GetShipperByIdAsync(model.Id)
                ?? throw new ArgumentException("No shipper found with the specified id");

            await _shipperService.DeleteShipperAsync(shipper);

            return new NullJsonResult();
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            foreach (var selectedId in selectedIds)
            {
                var shipper = await _shipperService.GetShipperByIdAsync(selectedId)
                ?? throw new ArgumentException("No shipper found with the specified id");

                await _shipperService.DeleteShipperAsync(shipper);
            }
            return Json(new { Result = true });
        }

        public async Task<IActionResult> ShipperAddPopup()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
                return await AccessDeniedDataTablesJson();

            var model = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ShipperAddPopup(AddShipperModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(DMSDefaults.ShipperCustomerRoleName);
            if (customerRole == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Shippers.CustomerRoleMissing"));
                return RedirectToAction("List");
            }

            var customers = await _customerService.GetCustomersByIdsAsync(model.SelectedCustomerIds.ToArray());

            foreach (var customer in customers)
            {
                var shipper = await _shipperService.GetShipperByCustomerIdAsync(customer.Id);
                if (shipper == null)
                {
                    await _shipperService.InsertShipperAsync(new Domain.Shipper()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Active = true,
                        CustomerId = customer.Id
                    });
                }

                if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
                    {
                        CustomerId = customer.Id,
                        CustomerRoleId = customerRole.Id
                    });
            }

            ViewBag.RefreshPage = true;

            return View(new CustomerSearchModel());
        }

        #endregion
    }
}
