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
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductTabs.Domains;
using NopStation.Plugin.Widgets.ProductTabs.Services;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Controllers
{
    public class ProductTabController : NopStationAdminController
    {
        #region Fields

        private readonly ISettingService _settingsService;
        private readonly ProductTabSettings _productTabSetting;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IProductTabService _productTabService;
        private readonly IProductTabModelFactory _productTabModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizedEntityService _localizedEntityService;

        #endregion

        #region Ctor

        public ProductTabController(ISettingService settingsService,
            ProductTabSettings productTabSetting,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IProductTabService productTabService,
            IProductTabModelFactory productTabModelFactory,
            IPermissionService permissionService,
            IProductService productService,
            IStoreContext storeContext,
            ISettingService settingService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ILocalizedEntityService localizedEntityService)
        {
            _settingsService = settingsService;
            _productTabSetting = productTabSetting;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _productTabService = productTabService;
            _productTabModelFactory = productTabModelFactory;
            _permissionService = permissionService;
            _productService = productService;
            _storeContext = storeContext;
            _settingService = settingService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _localizedEntityService = localizedEntityService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveStoreMappingsAsync(ProductTab carousel, ProductTabModel model)
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

        protected virtual async Task UpdateLocalesAsync(ProductTab productTab, ProductTabModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productTab,
                        x => x.Name,
                        localized.Name,
                        localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(productTab,
                        x => x.TabTitle,
                        localized.TabTitle,
                        localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(ProductTabItem productTabItem, ProductTabItemModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productTabItem,
                        x => x.Name,
                        localized.Name,
                        localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Configure

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var carouselSettings = await _settingService.LoadSettingAsync<ProductTabSettings>(storeId);

            var model = carouselSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.EnableProductTab_OverrideForStore = await _settingService.SettingExistsAsync(carouselSettings, x => x.EnableProductTab, storeId);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productTabSettings = await _settingService.LoadSettingAsync<ProductTabSettings>(storeScope);
            productTabSettings = model.ToSettings(productTabSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(productTabSettings, x => x.EnableProductTab, model.EnableProductTab_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region List

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var model = await _productTabModelFactory.PrepareOCarouselSearchModelAsync(new ProductTabSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ProductTabSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            var model = await _productTabModelFactory.PrepareProductTabListModelAsync(searchModel);
            return Json(model);
        }

        #endregion

        #region Create/update/delete

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var model = await _productTabModelFactory.PrepareProductTabModelAsync(new ProductTabModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProductTabModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productTab = model.ToEntity<ProductTab>();
                model.CreatedOn = DateTime.UtcNow;
                model.UpdatedOn = DateTime.UtcNow;
                productTab.CreatedOnUtc = DateTime.UtcNow;
                productTab.UpdatedOnUtc = DateTime.UtcNow;

                await _productTabService.InsertProductTabAsync(productTab);
                await UpdateLocalesAsync(productTab, model);

                await SaveStoreMappingsAsync(productTab, model);
                await _productTabService.UpdateProductTabAsync(productTab);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Created"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = productTab.Id }) :
                    RedirectToAction("List");
            }

            model = await _productTabModelFactory.PrepareProductTabModelAsync(model, null);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTab = await _productTabService.GetProductTabByIdAsync(id);
            if (productTab == null || productTab.Deleted)
                return RedirectToAction("List");

            var model = await _productTabModelFactory.PrepareProductTabModelAsync(null, productTab);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProductTabModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTab = await _productTabService.GetProductTabByIdAsync(model.Id);
            if (productTab == null || productTab.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                model.UpdatedOn = DateTime.UtcNow;
                productTab = model.ToEntity(productTab);
                productTab.UpdatedOnUtc = DateTime.UtcNow;
                await _productTabService.UpdateProductTabAsync(productTab);
                await UpdateLocalesAsync(productTab, model);

                await SaveStoreMappingsAsync(productTab, model);
                await _productTabService.UpdateProductTabAsync(productTab);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Updated"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = model.Id }) :
                    RedirectToAction("List");
            }

            model = await _productTabModelFactory.PrepareProductTabModelAsync(model, productTab);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(ProductTabModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTab = await _productTabService.GetProductTabByIdAsync(model.Id);
            if (productTab == null || productTab.Deleted)
                return RedirectToAction("List");

            await _productTabService.DeleteProductTabAsync(productTab);
            return RedirectToAction("List");
        }

        #endregion

        #region Product tab items

        [HttpPost]
        public virtual async Task<IActionResult> ItemList(ProductTabItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            //try to get a productTab with the specified id
            var productTab = await _productTabService.GetProductTabByIdAsync(searchModel.ProductTabId);
            if (productTab == null || productTab.Deleted)
                return new NullJsonResult();

            //prepare model
            var model = await _productTabModelFactory.PrepareProductTabItemListModelAsync(searchModel, productTab);

            return Json(model);
        }

        public virtual async Task<IActionResult> ItemCreate(int productTabId)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTab = await _productTabService.GetProductTabByIdAsync(productTabId);
            if (productTab == null || productTab.Deleted)
                return RedirectToAction("List");

            var model = new ProductTabItemModel();
            if (productTab != null)
            {
                model.Name = productTab.Name;
                model.ProductTabId = productTab.Id;
            }
            var viewModel = await _productTabModelFactory.PrepareProductTabItemModelAsync(model, null, productTab);

            return View(viewModel);

        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ItemCreate(ProductTabItemModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTab = await _productTabService.GetProductTabByIdAsync(model.ProductTabId);
            if (productTab == null || productTab.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                try
                {
                    var tabItem = new ProductTabItem();
                    tabItem.Name = model.Name;
                    tabItem.DisplayOrder = model.DisplayOrder;
                    tabItem.ProductTabId = model.ProductTabId;

                    await _productTabService.InsertProductTabItemAsync(tabItem);
                    await UpdateLocalesAsync(tabItem, model);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabItems.Created"));

                    return continueEditing ?
                        RedirectToAction("ItemEdit", new { id = tabItem.Id }) :
                        RedirectToAction("Edit", new { id = productTab.Id });
                }
                catch (Exception ex)
                {
                    string helloException = ex.InnerException.Message;
                }
            }

            model = await _productTabModelFactory.PrepareProductTabItemModelAsync(model, null, productTab);
            return View(model);
        }

        public virtual async Task<IActionResult> ItemEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(id);
            if (productTabItem == null)
                return RedirectToAction("List");

            var productTabItemModel = new ProductTabItemModel();

            productTabItemModel.Id = id;
            productTabItemModel.Name = productTabItem.Name;
            productTabItemModel.DisplayOrder = productTabItem.DisplayOrder;
            productTabItemModel.ProductTabId = productTabItem.ProductTabId;
            var model = await _productTabModelFactory.PrepareProductTabItemModelAsync(productTabItemModel, productTabItem, productTabItem.ProductTab);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ItemEdit(ProductTabItemModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTab = await _productTabService.GetProductTabByIdAsync(model.ProductTabId);
            if (productTab == null || productTab.Deleted)
                return RedirectToAction("List");

            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(model.Id);
            if (productTabItem == null)
                return RedirectToAction("Edit", new { id = productTab.Id });

            if (ModelState.IsValid)
            {
                productTabItem = model.ToEntity(productTabItem);

                await _productTabService.UpdateProductTabAsync(productTab);
                await _productTabService.UpdateProductTabItemAsync(productTabItem);

                await UpdateLocalesAsync(productTabItem, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabItems.Updated"));

                return continueEditing ?
                    RedirectToAction("ItemEdit", new { id = model.Id }) :
                    RedirectToAction("Edit", new { id = productTab.Id });
            }

            model = await _productTabModelFactory.PrepareProductTabItemModelAsync(model, productTabItem, productTab);
            return View(model);
        }

        [EditAccessAjax]
        public virtual async Task<IActionResult> ItemDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(id);
            if (productTabItem == null)
                return new NullJsonResult();

            await _productTabService.DeleteProductTabItemAsync(productTabItem);

            return new NullJsonResult();
        }

        #endregion

        #region Tab products

        [EditAccessAjax]
        public virtual async Task<IActionResult> ItemProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            var productTabItemProduct = await _productTabService.GetProductTabItemProductByIdAsync(id);
            if (productTabItemProduct == null)
                return new NullJsonResult();

            await _productTabService.DeleteProductTabItemProductAsync(productTabItemProduct);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> ItemProductUpdate(ProductTabItemProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            var productTabItemProduct = await _productTabService.GetProductTabItemProductByIdAsync(model.Id);
            if (productTabItemProduct == null)
                return new NullJsonResult();

            productTabItemProduct.DisplayOrder = model.DisplayOrder;
            await _productTabService.UpdateProductTabItemProductAsync(productTabItemProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ItemProductList(ProductTabItemProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            //try to get a productTab with the specified id
            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(searchModel.ProductTabItemId);
            if (productTabItem == null)
                return new NullJsonResult();

            //prepare model
            var model = await _productTabModelFactory.PrepareProductTabItemProductListModelAsync(searchModel, productTabItem);

            return Json(model);
        }

        public virtual async Task<IActionResult> ProductAddPopup(int productTabItemId)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(productTabItemId)
                ?? throw new ArgumentException("No product tab item found with the specified id");

            var model = await _productTabModelFactory.PrepareAddProductToProductTabItemSearchModelAsync(new AddProductToProductTabItemSearchModel());
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToProductTabItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return await AccessDeniedDataTablesJson();

            var model = await _productTabModelFactory.PrepareAddProductToProductTabItemListModelAsync(searchModel);
            return Json(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToProductTabItemModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProductTabPermissionProvider.ManageProductTab))
                return AccessDeniedView();

            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(model.ProductTabItemId)
                ?? throw new ArgumentException("No product tab item found with the specified id");

            var itemProducts = productTabItem.ProductTabItemProducts.ToList();

            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
                    if (!itemProducts.Any(x => x.ProductId == product.Id))
                    {
                        var productTabItemProduct = new ProductTabItemProduct()
                        {
                            ProductTabItemId = productTabItem.Id,
                            DisplayOrder = 0,
                            ProductId = product.Id,
                        };
                        await _productTabService.InsertProductTabItemProductAsync(productTabItemProduct);
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(new AddProductToProductTabItemSearchModel());
        }

        #endregion

        #endregion
    }
}
