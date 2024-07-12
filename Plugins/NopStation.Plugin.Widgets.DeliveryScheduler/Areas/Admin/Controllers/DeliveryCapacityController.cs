using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Controllers
{
    public class DeliveryCapacityController : NopStationAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IDeliveryCapacityModelFactory _deliveryCapacityModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IShippingService _shippingService;
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly IDeliveryCapacityService _deliveryCapacityService;
        private readonly IStoreContext _storeContext;

        public DeliveryCapacityController(IPermissionService permissionService,
            IDeliveryCapacityModelFactory deliveryCapacityModelFactory,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IShippingService shippingService,
            IDeliverySlotService deliverySlotService,
            IDeliveryCapacityService deliveryCapacityService,
            IStoreContext storeContext)
        {
            _permissionService = permissionService;
            _deliveryCapacityModelFactory = deliveryCapacityModelFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _shippingService = shippingService;
            _deliverySlotService = deliverySlotService;
            _deliveryCapacityService = deliveryCapacityService;
            _storeContext = storeContext;
        }

        protected int GetSlotCapacity(IFormCollection form, int slotId, int weekOfDay)
        {
            var key = $"Capacity_{slotId}_{weekOfDay}";
            if (form.ContainsKey(key) && int.TryParse(form[key].ToString(), out var cap))
                return cap;

            return 0;
        }

        public async Task<IActionResult> Configure(int shippingMethodId = 0)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(shippingMethodId);
            if (shippingMethod == null)
            {
                var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
                shippingMethod = shippingMethods.FirstOrDefault();
            }

            if (shippingMethod == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.NoShippingMethod"));
                return View(new DeliveryCapacityConfigurationModel());
            }

            var model = await _deliveryCapacityModelFactory.PrepareConfigurationModelAsync(shippingMethod);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(int shippingMethodId, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(shippingMethodId);
            if (shippingMethod == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.NoShippingMethod"));
                return RedirectToAction("Configure");
            }

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var slots = await _deliverySlotService.SearchDeliverySlotsAsync(storeId: storeScope);
            foreach (var slot in slots)
            {
                var capacity = await _deliveryCapacityService.GetDeliveryCapacityAsync(slot.Id, shippingMethodId);
                if (capacity == null)
                {
                    capacity = new Domains.DeliveryCapacity()
                    {
                        Day1Capacity = GetSlotCapacity(form, slot.Id, 1),
                        Day2Capacity = GetSlotCapacity(form, slot.Id, 2),
                        Day3Capacity = GetSlotCapacity(form, slot.Id, 3),
                        Day4Capacity = GetSlotCapacity(form, slot.Id, 4),
                        Day5Capacity = GetSlotCapacity(form, slot.Id, 5),
                        Day6Capacity = GetSlotCapacity(form, slot.Id, 6),
                        Day7Capacity = GetSlotCapacity(form, slot.Id, 7),
                        ShippingMethodId = shippingMethodId,
                        DeliverySlotId = slot.Id
                    };
                    await _deliveryCapacityService.InsertDeliveryCapacityAsync(capacity);
                }
                else
                {
                    capacity.Day1Capacity = GetSlotCapacity(form, slot.Id, 1);
                    capacity.Day2Capacity = GetSlotCapacity(form, slot.Id, 2);
                    capacity.Day3Capacity = GetSlotCapacity(form, slot.Id, 3);
                    capacity.Day4Capacity = GetSlotCapacity(form, slot.Id, 4);
                    capacity.Day5Capacity = GetSlotCapacity(form, slot.Id, 5);
                    capacity.Day6Capacity = GetSlotCapacity(form, slot.Id, 6);
                    capacity.Day7Capacity = GetSlotCapacity(form, slot.Id, 7);
                    capacity.ShippingMethodId = shippingMethodId;
                    capacity.DeliverySlotId = slot.Id;

                    await _deliveryCapacityService.UpdateDeliveryCapacityAsync(capacity);
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.DeliveryCapacities.Updated"));
            return RedirectToAction("Configure", new { shippingMethodId = shippingMethodId });
        }
    }
}
