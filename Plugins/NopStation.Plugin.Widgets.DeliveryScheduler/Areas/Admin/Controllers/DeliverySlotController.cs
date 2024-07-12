using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Controllers
{
    public class DeliverySlotController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IDeliverySlotModelFactory _deliverySlotModelFactory;
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IShippingService _shippingService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public DeliverySlotController(ILocalizationService localizationService,
            INotificationService notificationService,
            IDeliverySlotModelFactory deliverySlotModelFactory,
            IDeliverySlotService deliverySlotService,
            IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService,
            IShippingService shippingService,
            IStoreMappingService storeMappingService,
            IStoreService storeService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _deliverySlotModelFactory = deliverySlotModelFactory;
            _deliverySlotService = deliverySlotService;
            _permissionService = permissionService;
            _localizedEntityService = localizedEntityService;
            _shippingService = shippingService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(DeliverySlot deliverySlot, DeliverySlotModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(deliverySlot,
                    x => x.TimeSlot,
                    localized.TimeSlot,
                    localized.LanguageId);
            }
        }

        protected virtual async Task SaveShippingMethodMappingsAsync(DeliverySlot deliverySlot, DeliverySlotModel model)
        {
            deliverySlot.LimitedToShippingMethod = model.SelectedShippingMethodIds.Any();
            await _deliverySlotService.UpdateDeliverySlotAsync(deliverySlot);

            var existingStoreMappings = await _deliverySlotService.GetDeliverySlotShippingMethodMappingsAsync(deliverySlot);
            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            foreach (var shippingMethod in shippingMethods)
            {
                if (model.SelectedShippingMethodIds.Contains(shippingMethod.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.ShippingMethodId == shippingMethod.Id) == 0)
                        await _deliverySlotService.InsertDeliverySlotShippingMethodMappingAsync(deliverySlot, shippingMethod.Id);
                }
                else
                {
                    //remove store
                    var slotShippingMethodMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.ShippingMethodId == shippingMethod.Id);
                    if (slotShippingMethodMappingToDelete != null)
                        await _deliverySlotService.DeleteDeliverySlotShippingMethodMappingAsync(slotShippingMethodMappingToDelete);
                }
            }
        }

        protected virtual async Task SaveStoreMappingsAsync(DeliverySlot deliverySlot, DeliverySlotModel model)
        {
            deliverySlot.LimitedToStores = model.SelectedStoreIds.Any();
            await _deliverySlotService.UpdateDeliverySlotAsync(deliverySlot);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(deliverySlot);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(deliverySlot, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            var searchModel = await _deliverySlotModelFactory.PrepareDeliverySlotSearchModelAsync(new DeliverySlotSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(DeliverySlotSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            var model = await _deliverySlotModelFactory.PrepareDeliverySlotListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            var model = await _deliverySlotModelFactory.PrepareDeliverySlotModelAsync(new DeliverySlotModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(DeliverySlotModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var deliverySlot = model.ToEntity<DeliverySlot>();
                deliverySlot.CreatedOnUtc = DateTime.UtcNow;

                await _deliverySlotService.InsertDeliverySlotAsync(deliverySlot);
                await UpdateLocalesAsync(deliverySlot, model);
                await SaveShippingMethodMappingsAsync(deliverySlot, model);
                await SaveStoreMappingsAsync(deliverySlot, model);
                await SaveShippingMethodMappingsAsync(deliverySlot, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.DeliverySlots.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = deliverySlot.Id });
            }

            model = await _deliverySlotModelFactory.PrepareDeliverySlotModelAsync(new DeliverySlotModel(), null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            var deliverySlot = await _deliverySlotService.GetDeliverySlotByIdAsync(id);
            if (deliverySlot == null || deliverySlot.Deleted)
                return RedirectToAction("List");

            var model = await _deliverySlotModelFactory.PrepareDeliverySlotModelAsync(null, deliverySlot);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(DeliverySlotModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            var deliverySlot = await _deliverySlotService.GetDeliverySlotByIdAsync(model.Id);
            if (deliverySlot == null || deliverySlot.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                deliverySlot = model.ToEntity(deliverySlot);
                await _deliverySlotService.UpdateDeliverySlotAsync(deliverySlot);
                await UpdateLocalesAsync(deliverySlot, model);
                await SaveShippingMethodMappingsAsync(deliverySlot, model);
                await SaveStoreMappingsAsync(deliverySlot, model);
                await SaveShippingMethodMappingsAsync(deliverySlot, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.DeliverySlots.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = deliverySlot.Id });
            }

            model = await _deliverySlotModelFactory.PrepareDeliverySlotModelAsync(model, deliverySlot);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliverySlots))
                return AccessDeniedView();

            var deliverySlot = await _deliverySlotService.GetDeliverySlotByIdAsync(id);
            if (deliverySlot == null)
                return RedirectToAction("List");

            await _deliverySlotService.DeleteDeliverySlotAsync(deliverySlot);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.DeliverySlots.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
