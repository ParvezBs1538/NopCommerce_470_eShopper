using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Controllers
{
    public class AnywhereSliderController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISliderModelFactory _sliderModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly SliderSettings _sliderSettings;
        private readonly ISliderService _sliderService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public AnywhereSliderController(IStoreContext storeContext,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreMappingService storeMappingService,
            ISliderModelFactory sliderModelFactory,
            IPermissionService permissionService,
            IPictureService pictureService,
            ISettingService settingService,
            SliderSettings sliderSettings,
            ISliderService sliderService,
            IStoreService storeService)
        {
            _storeContext = storeContext;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeMappingService = storeMappingService;
            _sliderModelFactory = sliderModelFactory;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _sliderSettings = sliderSettings;
            _settingService = settingService;
            _sliderService = sliderService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveStoreMappingsAsync(Slider slider, SliderModel model)
        {
            slider.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(slider);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(slider, store.Id);
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

        protected virtual async Task UpdateLocalesAsync(Slider slider, SliderModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(slider,
                        x => x.Name,
                        localized.Name,
                        localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(SliderItem sliderItem, SliderItemModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                    x => x.Title,
                    localized.SliderItemTitle,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                    x => x.Link,
                    localized.Link,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                    x => x.ShopNowLink,
                    localized.ShopNowLink,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(sliderItem,
                    x => x.ImageAltText,
                    localized.ImageAltText,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var model = await _sliderModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sliderSettings = await _settingService.LoadSettingAsync<SliderSettings>(storeScope);
            sliderSettings = model.ToSettings(sliderSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(sliderSettings, x => x.EnableSlider, model.EnableSlider_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region List

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var model = await _sliderModelFactory.PrepareSliderSearchModelAsync(new SliderSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(SliderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return await AccessDeniedDataTablesJson();

            var sliders = await _sliderModelFactory.PrepareSliderListModelAsync(searchModel);
            return Json(sliders);
        }

        #endregion

        #region Create / update / delete

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var model = await _sliderModelFactory.PrepareSliderModelAsync(new SliderModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(SliderModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var slider = model.ToEntity<Slider>();
                slider.CreatedOnUtc = DateTime.UtcNow;
                slider.UpdatedOnUtc = DateTime.UtcNow;

                await _sliderService.InsertSliderAsync(slider);

                await UpdateLocalesAsync(slider, model);

                await SaveStoreMappingsAsync(slider, model);

                await _sliderService.UpdateSliderAsync(slider);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Created"));

                return continueEditing
                    ? RedirectToAction("Edit", new { id = slider.Id })
                    : RedirectToAction("List");
            }
            model = await _sliderModelFactory.PrepareSliderModelAsync(model, null);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var slider = await _sliderService.GetSliderByIdAsync(id);
            if (slider == null || slider.Deleted)
                return RedirectToAction("List");

            var model = await _sliderModelFactory.PrepareSliderModelAsync(null, slider);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SliderModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var slider = await _sliderService.GetSliderByIdAsync(model.Id);
            if (slider == null || slider.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                slider = model.ToEntity(slider);
                slider.UpdatedOnUtc = DateTime.UtcNow;

                await _sliderService.UpdateSliderAsync(slider);

                await UpdateLocalesAsync(slider, model);

                await SaveStoreMappingsAsync(slider, model);

                await _sliderService.UpdateSliderAsync(slider);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Updated"));

                return continueEditing
                    ? RedirectToAction("Edit", new { id = model.Id })
                    : RedirectToAction("List");
            }

            model = await _sliderModelFactory.PrepareSliderModelAsync(model, slider);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var slider = await _sliderService.GetSliderByIdAsync(id);
            if (slider == null || slider.Deleted)
                return RedirectToAction("List");

            await _sliderService.DeleteSliderAsync(slider);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Slider items

        public virtual async Task<IActionResult> SliderItemCreatePopup(int sliderId)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            var slider = await _sliderService.GetSliderByIdAsync(sliderId)
                ?? throw new ArgumentException("No slider found with the specified id", nameof(sliderId));

            //prepare model
            var model = await _sliderModelFactory.PrepareSliderItemModelAsync(new SliderItemModel(), slider, null);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> SliderItemCreatePopup(SliderItemModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            //try to get a slider with the specified id
            var slider = await _sliderService.GetSliderByIdAsync(model.SliderId)
                ?? throw new ArgumentException("No slider found with the specified id");

            if (ModelState.IsValid)
            {
                //fill entity from model
                var sliderItem = model.ToEntity<SliderItem>();

                await _sliderService.InsertSliderItemAsync(sliderItem);
                await UpdateLocalesAsync(sliderItem, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _sliderModelFactory.PrepareSliderItemModelAsync(model, slider, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> SliderItemEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            //try to get a predefined slider value with the specified id
            var sliderItem = await _sliderService.GetSliderItemByIdAsync(id)
                ?? throw new ArgumentException("No slider item found with the specified id");

            //try to get a slider with the specified id
            var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)
                ?? throw new ArgumentException("No slider found with the specified id");

            //prepare model
            var model = await _sliderModelFactory.PrepareSliderItemModelAsync(null, slider, sliderItem);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> SliderItemEditPopup(SliderItemModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return AccessDeniedView();

            //try to get a predefined slider value with the specified id
            var sliderItem = await _sliderService.GetSliderItemByIdAsync(model.Id)
                ?? throw new ArgumentException("No slider item found with the specified id");

            //try to get a slider with the specified id
            var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)
                ?? throw new ArgumentException("No slider found with the specified id");

            if (ModelState.IsValid)
            {
                sliderItem = model.ToEntity(sliderItem);
                sliderItem.Title = model.SliderItemTitle;
                await _sliderService.UpdateSliderItemAsync(sliderItem);

                await UpdateLocalesAsync(sliderItem, model);
                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _sliderModelFactory.PrepareSliderItemModelAsync(model, slider, sliderItem, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> SliderItemDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return await AccessDeniedDataTablesJson();

            var sliderItem = await _sliderService.GetSliderItemByIdAsync(id)
                ?? throw new ArgumentException("No slider item found with the specified id");

            var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId);
            if (slider.Deleted)
                return new NullJsonResult();

            await _sliderService.DeleteSliderItemAsync(sliderItem);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> SliderItemList(SliderItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
                return await AccessDeniedDataTablesJson();

            var listModel = await _sliderModelFactory.PrepareSliderItemListModelAsync(searchModel);
            return Json(listModel);
        }

        #endregion   

        #endregion        
    }
}