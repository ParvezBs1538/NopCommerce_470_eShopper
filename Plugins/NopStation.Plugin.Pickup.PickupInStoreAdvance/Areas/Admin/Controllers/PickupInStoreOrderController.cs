using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Factories;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Controllers
{
    public class PickupInStoreOrderController : NopStationAdminController
    {
        private readonly IPickupInStoreDeliveryManageModelFactory _pickupInStoreDeliveryManageModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public PickupInStoreOrderController(IPickupInStoreDeliveryManageModelFactory pickupInStoreDeliveryManageModelFactory, IPermissionService permissionService,
            IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _pickupInStoreDeliveryManageModelFactory = pickupInStoreDeliveryManageModelFactory;
            _permissionService = permissionService;
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        public async Task<IActionResult> MarkOrderAsReadyAsync(int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var markedAsReady = await _pickupInStoreDeliveryManageService.MarkAsReadyToPickupAsync(orderId);
            if (!markedAsReady)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.UnSuccess"));
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.Success"));
            return RedirectToAction("Edit", "Order", new { id = orderId });
        }

        public async Task<IActionResult> MarkOrderAsPickedUpAsync(int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var markedAsReady = await _pickupInStoreDeliveryManageService.MarkAsPickedByCustomerAsync(orderId);
            if (!markedAsReady)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.UnSuccess"));
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.Success"));
            return RedirectToAction("Edit", "Order", new { id = orderId });
        }

        public async Task<IActionResult> OrdersAsync()
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            var model = _pickupInStoreDeliveryManageModelFactory.PreparePickupInStoreDeliveryManageSearchModel(new PickupInStoreDeliveryManageSearchModel());

            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> OrderListAsync(PickupInStoreDeliveryManageSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            var model = await _pickupInStoreDeliveryManageModelFactory.PreparePickupInStoreDeliveryManageListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> MarkReadyAsync(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var markedAsReady = await _pickupInStoreDeliveryManageService.MarkAsReadyToPickupAsync(id);
            if (!markedAsReady)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.UnSuccess"));
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.Success"));
            return RedirectToAction("Orders");
        }

        public async Task<IActionResult> MarkPickedAsync(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            var markedAsReady = await _pickupInStoreDeliveryManageService.MarkAsPickedByCustomerAsync(id);
            if (!markedAsReady)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.UnSuccess"));
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.Success"));
            return RedirectToAction("Orders");
        }
    }
}
