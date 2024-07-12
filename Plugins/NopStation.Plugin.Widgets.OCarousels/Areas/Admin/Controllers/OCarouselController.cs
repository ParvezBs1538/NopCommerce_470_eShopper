using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Controllers
{
    public partial class OCarouselController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IOCarouselModelFactory _carouselModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly OCarouselSettings _carouselSetting;
        private readonly IOCarouselService _carouselService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public OCarouselController(IStoreContext storeContext,
            ILocalizedEntityService localizedEntityService,
            IOCarouselModelFactory carouselModelFactory,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            OCarouselSettings carouselSetting,
            IOCarouselService carouselService,
            ISettingService settingService,
            IProductService productService,
            IStoreService storeService,
            IWorkContext workContext)
        {
            _storeContext = storeContext;
            _localizedEntityService = localizedEntityService;
            _carouselModelFactory = carouselModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _carouselSetting = carouselSetting;
            _carouselService = carouselService;
            _settingService = settingService;
            _productService = productService;
            _storeService = storeService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveStoreMappingsAsync(OCarousel carousel, OCarouselModel model)
        {
            carousel.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(carousel);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(carousel, store.Id);
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

        protected virtual async Task UpdateLocalesAsync(OCarousel oCarousel, OCarouselModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(oCarousel,
                         x => x.Title,
                         localized.Title,
                         localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Configure

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var model = await _carouselModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var carouselSettings = await _settingService.LoadSettingAsync<OCarouselSettings>(storeScope);
            carouselSettings = model.ToSettings(carouselSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(carouselSettings, x => x.EnableOCarousel, model.EnableOCarousel_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region List

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var model = await _carouselModelFactory.PrepareOCarouselSearchModelAsync(new OCarouselSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(OCarouselSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return await AccessDeniedDataTablesJson();

            var model = await _carouselModelFactory.PrepareOCarouselListModelAsync(searchModel);
            return Json(model);
        }

        #endregion

        #region Create/update/delete

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var model = await _carouselModelFactory.PrepareOCarouselModelAsync(new OCarouselModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Create(OCarouselModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var carousel = model.ToEntity<OCarousel>();
                carousel.CreatedOnUtc = DateTime.UtcNow;
                carousel.UpdatedOnUtc = DateTime.UtcNow;

                await _carouselService.InsertCarouselAsync(carousel);

                await UpdateLocalesAsync(carousel, model);

                await SaveStoreMappingsAsync(carousel, model);

                await _carouselService.UpdateCarouselAsync(carousel);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Created"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = carousel.Id }) :
                    RedirectToAction("List");
            }

            model = await _carouselModelFactory.PrepareOCarouselModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var carousel = await _carouselService.GetCarouselByIdAsync(id);
            if (carousel == null || carousel.Deleted)
                return RedirectToAction("List");

            var model = await _carouselModelFactory.PrepareOCarouselModelAsync(null, carousel);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Edit(OCarouselModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var carousel = await _carouselService.GetCarouselByIdAsync(model.Id);
            if (carousel == null || carousel.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                carousel = model.ToEntity(carousel);
                carousel.UpdatedOnUtc = DateTime.UtcNow;

                await _carouselService.UpdateCarouselAsync(carousel);

                await UpdateLocalesAsync(carousel, model);

                await SaveStoreMappingsAsync(carousel, model);

                await _carouselService.UpdateCarouselAsync(carousel);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Updated"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = model.Id }) :
                    RedirectToAction("List");
            }

            model = await _carouselModelFactory.PrepareOCarouselModelAsync(model, carousel);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async virtual Task<IActionResult> Delete(OCarouselModel model)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var carousel = await _carouselService.GetCarouselByIdAsync(model.Id);
            if (carousel == null || carousel.Deleted)
                return RedirectToAction("List");

            await _carouselService.DeleteCarouselAsync(carousel);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Ocarousel items

        [HttpPost]
        public async virtual Task<IActionResult> OCarouselItemList(OCarouselItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return await AccessDeniedDataTablesJson();

            //try to get a carousel with the specified id
            var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.OCarouselId);
            if (carousel == null || carousel.Deleted)
                return new NullJsonResult();

            //prepare model
            var model = await _carouselModelFactory.PrepareOCarouselItemListModelAsync(searchModel, carousel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async virtual Task<IActionResult> OCarouselItemEdit(OCarouselItemModel model)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return await AccessDeniedDataTablesJson();

            var carousel = await _carouselService.GetCarouselByIdAsync(model.OCarouselId);
            if (carousel == null || carousel.Deleted)
                return new NullJsonResult();

            var carouselItem = (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(carousel.Id)).FirstOrDefault(x => x.Id == model.Id)
                ?? throw new ArgumentException("No carousel item found with the specified id", nameof(model.Id));

            //remove carousel item
            carouselItem.DisplayOrder = model.DisplayOrder;
            await _carouselService.UpdateOCarouselItemAsync(carouselItem);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async virtual Task<IActionResult> OCarouselItemDelete(int ocarouselId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return await AccessDeniedDataTablesJson();

            var carousel = await _carouselService.GetCarouselByIdAsync(ocarouselId);
            if (carousel == null || carousel.Deleted)
                return new NullJsonResult();

            var carouselItem = (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(carousel.Id)).FirstOrDefault(x => x.Id == id)
                ?? throw new ArgumentException("No carousel item found with the specified id", nameof(id));

            //remove carousel item
            await _carouselService.DeleteOCarouselItemAsync(carouselItem);

            return new NullJsonResult();
        }

        public async virtual Task<IActionResult> ProductAddPopup(int ocarouselId)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var carousel = await _carouselService.GetCarouselByIdAsync(ocarouselId)
                ?? throw new ArgumentException("No carousel found with the specified id");

            if (carousel.Deleted)
                throw new ArgumentException("No carousel found with the specified id");

            var model = await _carouselModelFactory.PrepareAddProductToOCarouselSearchModelAsync(new AddProductToCarouselSearchModel());
            return View(model);
        }

        [HttpPost]
        public async virtual Task<IActionResult> ProductAddPopupList(AddProductToCarouselSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return await AccessDeniedDataTablesJson();

            var model = await _carouselModelFactory.PrepareAddProductToOCarouselListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async virtual Task<IActionResult> ProductAddPopup(AddProductToCarouselModel model)
        {
            if (!await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
                return AccessDeniedView();

            var carousel = await _carouselService.GetCarouselByIdAsync(model.OCarouselId)
                ?? throw new ArgumentException("No carousel found with the specified id");

            if (carousel.Deleted)
                throw new ArgumentException("No carousel found with the specified id");

            var carouselItems = await _carouselService.GetOCarouselItemsByOCarouselIdAsync(model.OCarouselId)
                ?? throw new ArgumentException("No carousel item found with the specified id", nameof(model.OCarouselId));

            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
                    if (!carouselItems.Any(x => x.ProductId == product.Id))
                    {
                        await _carouselService.InsertOCarouselItemAsync(new OCarouselItem()
                        {
                            DisplayOrder = 0,
                            OCarouselId = model.OCarouselId,
                            ProductId = product.Id,
                        });
                    }
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToCarouselSearchModel());
        }

        #endregion

        #endregion
    }
}
