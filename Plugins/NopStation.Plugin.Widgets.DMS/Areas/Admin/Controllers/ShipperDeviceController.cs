using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipperDevice;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Controllers
{
    public class ShipperDeviceController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IDeviceModelFactory _deviceModelFactory;
        private readonly IShipperDeviceService _deviceService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public ShipperDeviceController(ILocalizationService localizationService,
            INotificationService notificationService,
            IDeviceModelFactory deviceModelFactory,
            IShipperDeviceService deviceService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _deviceModelFactory = deviceModelFactory;
            _deviceService = deviceService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return AccessDeniedView();

            var searchModel = await _deviceModelFactory.PrepareDeviceSearchModelAsync(new DeviceSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(DeviceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return AccessDeniedView();

            var model = await _deviceModelFactory.PrepareDeviceListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> View(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return AccessDeniedView();

            var device = await _deviceService.GetShipperDeviceByIdAsync(id);
            if (device == null)
                return RedirectToAction("List");

            var model = await _deviceModelFactory.PrepareDeviceModelAsync(null, device);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return AccessDeniedView();

            var device = await _deviceService.GetShipperDeviceByIdAsync(id);
            if (device == null)
                return RedirectToAction("List");

            await _deviceService.DeleteShipperDeviceAsync(device);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Devices.Deleted"));

            return RedirectToAction("List");
        }

        [EditAccessAjax]
        public async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return AccessDeniedView();

            if (selectedIds != null)
                await _deviceService.DeleteShipperDevicesAsync(_deviceService.GetShipperDeviceByIds(selectedIds.ToArray()).ToList());

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> GetLocation(int customerId)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
                return await AccessDeniedDataTablesJson();

            var device = await _deviceService.GetShipperDeviceByCustomerIdAsync(customerId);
            if (device != null)
            {
                return Json(new { Result = true, lat = device.Latitude, lng = device.Longitude });
            }

            return Json(new { Result = false });

        }

        #endregion
    }
}
