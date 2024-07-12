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
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Controllers
{
    public class SpecialDeliveryCapacityController : NopStationAdminController
    {
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISpecialDeliveryCapacityService _specialDeliveryCapacityService;
        private readonly ISpecialDeliveryCapacityModelFactory _specialDeliveryCapacityModelFactory;

        public SpecialDeliveryCapacityController(
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ISpecialDeliveryCapacityService specialDeliveryCapacityService,
            ISpecialDeliveryCapacityModelFactory specialDeliveryCapacityModelFactory)
        {
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _specialDeliveryCapacityService = specialDeliveryCapacityService;
            _specialDeliveryCapacityModelFactory = specialDeliveryCapacityModelFactory;
        }

        #region Utilities

        protected virtual async Task SaveStoreMappingsAsync(SpecialDeliveryCapacity specialDeliveryCapacity, SpecialDeliveryCapacityModel model)
        {
            specialDeliveryCapacity.LimitedToStores = model.SelectedStoreIds.Any();
            await _specialDeliveryCapacityService.UpdateSpecialDeliveryCapacityAsync(specialDeliveryCapacity);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(specialDeliveryCapacity);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(specialDeliveryCapacity, store.Id);
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
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var searchModel = await _specialDeliveryCapacityModelFactory.PrepareCapacitySearchModelAsync(new SpecialDeliveryCapacitySearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(SpecialDeliveryCapacitySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var model = await _specialDeliveryCapacityModelFactory.PrepareCapacityListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var model = await _specialDeliveryCapacityModelFactory.PrepareCapacityModelAsync(new SpecialDeliveryCapacityModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(SpecialDeliveryCapacityModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var specialDeliveryCapacity = model.ToEntity<SpecialDeliveryCapacity>();

                await _specialDeliveryCapacityService.InsertSpecialDeliveryCapacityAsync(specialDeliveryCapacity);

                await SaveStoreMappingsAsync(specialDeliveryCapacity, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = specialDeliveryCapacity.Id });
            }

            model = await _specialDeliveryCapacityModelFactory.PrepareCapacityModelAsync(model, null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var specialDeliveryCapacity = await _specialDeliveryCapacityService.GetSpecialDeliveryCapacityByIdAsync(id);
            if (specialDeliveryCapacity == null)
                return RedirectToAction("List");

            var model = await _specialDeliveryCapacityModelFactory.PrepareCapacityModelAsync(null, specialDeliveryCapacity);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SpecialDeliveryCapacityModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var specialDeliveryCapacity = await _specialDeliveryCapacityService.GetSpecialDeliveryCapacityByIdAsync(model.Id);
            if (specialDeliveryCapacity == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                specialDeliveryCapacity = model.ToEntity(specialDeliveryCapacity);
                await _specialDeliveryCapacityService.UpdateSpecialDeliveryCapacityAsync(specialDeliveryCapacity);

                await SaveStoreMappingsAsync(specialDeliveryCapacity, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = specialDeliveryCapacity.Id });
            }

            model = await _specialDeliveryCapacityModelFactory.PrepareCapacityModelAsync(model, specialDeliveryCapacity);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageDeliveryCapacity))
                return AccessDeniedView();

            var specialDeliveryCapacity = await _specialDeliveryCapacityService.GetSpecialDeliveryCapacityByIdAsync(id);
            if (specialDeliveryCapacity == null)
                return RedirectToAction("List");

            await _specialDeliveryCapacityService.DeleteSpecialDeliveryCapacityAsync(specialDeliveryCapacity);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.DeliveryScheduler.SpecialDeliveryCapacities.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
