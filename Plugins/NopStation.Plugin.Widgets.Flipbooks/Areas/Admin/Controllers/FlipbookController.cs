using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Flipbooks.Services;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Core.Caching;
using Nop.Web.Areas.Admin.Factories;
using Nop.Services.Logging;
using Nop.Services.Catalog;
using System.Threading.Tasks;
using Nop.Services.Security;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models;
using Nop.Services.Localization;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using System;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Controllers;
using System.Linq;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Services.Helpers;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Customers;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Controllers
{
    public class FlipbookController : NopStationAdminController
    {
        #region Fields
        
        private readonly IFlipbookService _flipbookService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly IPictureService _pictureService;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly FlipbookSettings _catalogSettings;
        private readonly IFlipbookModelFactory _flipbookModelFactory;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;

        #endregion

        #region Ctor

        public FlipbookController(IFlipbookService flipbookService,
            ICategoryModelFactory categoryModelFactory,
            IPictureService pictureService,
            ILogger logger,
            IProductService productService,
            IStaticCacheManager cacheManager,
            FlipbookSettings catalogSettings,
            IFlipbookModelFactory flipbookModelFactory,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ICustomerService customerService,
            IAclService aclService)
        {
            _flipbookService = flipbookService;
            _categoryModelFactory = categoryModelFactory;
            _pictureService = pictureService;
            _logger = logger;
            _cacheManager = cacheManager;
            _catalogSettings = catalogSettings;
            _productService = productService;
            _flipbookModelFactory = flipbookModelFactory;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _customerService = customerService;
            _aclService = aclService;
        }

        #endregion
        
        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalesAsync(Flipbook flipbook, FlipbookModel model) 
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(flipbook,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(flipbook,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(flipbook,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(flipbook,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(flipbook, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(flipbook, seName, localized.LanguageId);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveFlipbookAclAsync(Flipbook flipbook, FlipbookModel model)
        {
            flipbook.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _flipbookService.UpdateFlipbookAsync(flipbook);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(flipbook);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(flipbook, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected virtual async Task SaveStoreMappingsAsync(Flipbook flipbook, FlipbookModel model)
        {
            flipbook.LimitedToStores = model.SelectedStoreIds.Any();
            await _flipbookService.UpdateFlipbookAsync(flipbook);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(flipbook);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(flipbook, store.Id);
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

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var flipbookSettings = await _settingService.LoadSettingAsync<FlipbookSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                DefaultPageSize = flipbookSettings.DefaultPageSize
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.DefaultPageSize_OverrideForStore = await _settingService.SettingExistsAsync(flipbookSettings, x => x.DefaultPageSize, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var flipbookSettings = await _settingService.LoadSettingAsync<FlipbookSettings>(storeScope);

            flipbookSettings.DefaultPageSize = model.DefaultPageSize;

            await _settingService.SaveSettingOverridablePerStoreAsync(flipbookSettings, x => x.DefaultPageSize, model.DefaultPageSize_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region Flipbook

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var searchModel = await _flipbookModelFactory.PrepareFlipbookSearchModelAsync(new FlipbookSearchModel());

            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> List(FlipbookSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var model = await _flipbookModelFactory.PrepareFlipbookListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var model = await _flipbookModelFactory.PrepareFlipbookModelAsync(new FlipbookModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(FlipbookModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var flipbook = model.ToEntity<Flipbook>();
                flipbook.CreatedOnUtc = DateTime.UtcNow;
                flipbook.UpdatedOnUtc = DateTime.UtcNow;

                flipbook.AvailableEndDateTimeUtc = !model.AvailableEndDateTime.HasValue ? (DateTime?)null :
                    _dateTimeHelper.ConvertToUtcTime(model.AvailableEndDateTime.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                flipbook.AvailableStartDateTimeUtc = !model.AvailableStartDateTime.HasValue ? (DateTime?)null :
                    _dateTimeHelper.ConvertToUtcTime(model.AvailableStartDateTime.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

                await _flipbookService.InsertFlipbookAsync(flipbook);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(flipbook, model.SeName, flipbook.Name, true);
                await _urlRecordService.SaveSlugAsync(flipbook, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(flipbook, model);

                //stores
                await SaveStoreMappingsAsync(flipbook, model);

                //ACL (customer roles)
                await SaveFlipbookAclAsync(flipbook, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = flipbook.Id });
            }

            model = await _flipbookModelFactory.PrepareFlipbookModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(id);
            if(flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            var model = await _flipbookModelFactory.PrepareFlipbookModelAsync(null, flipbook);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(FlipbookModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(model.Id);
            if (flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                flipbook = model.ToEntity(flipbook);
                flipbook.UpdatedOnUtc = DateTime.UtcNow;

                flipbook.AvailableEndDateTimeUtc = !model.AvailableEndDateTime.HasValue ? (DateTime?)null :
                    _dateTimeHelper.ConvertToUtcTime(model.AvailableEndDateTime.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                flipbook.AvailableStartDateTimeUtc = !model.AvailableStartDateTime.HasValue ? (DateTime?)null :
                    _dateTimeHelper.ConvertToUtcTime(model.AvailableStartDateTime.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

                await _flipbookService.UpdateFlipbookAsync(flipbook);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(flipbook, model.SeName, flipbook.Name, true);
                await _urlRecordService.SaveSlugAsync(flipbook, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(flipbook, model);

                //stores
                await SaveStoreMappingsAsync(flipbook, model);

                //ACL (customer roles)
                await SaveFlipbookAclAsync(flipbook, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = flipbook.Id });
            }

            model = await _flipbookModelFactory.PrepareFlipbookModelAsync(model, null);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(id);
            if (flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            //delete a flipbook
            await _flipbookService.DeleteFlipbookAsync(flipbook);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Contents

        [HttpPost]
        public async Task<IActionResult> GetContents(FlipbookContentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return await AccessDeniedDataTablesJson();

            var model = await _flipbookModelFactory.PrepareFlipbookContentListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> ContentCreate(int flipbookId)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(flipbookId);
            if (flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            var model = await _flipbookModelFactory.PrepareFlipbookContentModelAsync(new FlipbookContentModel(), null, flipbook);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> ContentCreate(FlipbookContentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(model.FlipbookId);
            if (flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var content = model.ToEntity<FlipbookContent>();
                await _flipbookService.InsertFlipbookContentAsync(content);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.FlipbookContents.Created"));

                if (!continueEditing)
                    return RedirectToAction("Edit", new { id = content.FlipbookId });

                return RedirectToAction("ContentEdit", new { id = content.Id });
            }

            model = await _flipbookModelFactory.PrepareFlipbookContentModelAsync(model, null, flipbook);
            return View(model);
        }

        public async Task<IActionResult> ContentEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var content = await _flipbookService.GetFlipbookContentByIdAsync(id);
            if (content == null)
                return RedirectToAction("List");

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(content.FlipbookId);
            if (flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            var model = await _flipbookModelFactory.PrepareFlipbookContentModelAsync(null, content, flipbook);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> ContentEdit(FlipbookContentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return await AccessDeniedDataTablesJson();

            var content = await _flipbookService.GetFlipbookContentByIdAsync(model.Id);
            if (content == null)
                return RedirectToAction("List");

            var flipbook = await _flipbookService.GetFlipbookByIdAsync(content.FlipbookId);
            if (flipbook == null || flipbook.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                content = model.ToEntity(content);

                await _flipbookService.UpdateFlipbookContentAsync(content);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.FlipbookContents.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", new { id = content.FlipbookId });

                return RedirectToAction("ContentEdit", new { id = content.Id });
            }

            model = await _flipbookModelFactory.PrepareFlipbookContentModelAsync(model, content, flipbook);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> ContentDelete(FlipbookContentModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            var content = await _flipbookService.GetFlipbookContentByIdAsync(model.Id);

            if(content == null)
                return RedirectToAction("List");

            await _flipbookService.DeleteFlipbookContentAsync(content);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.FlipbookContents.Deleted"));

            return RedirectToAction("Content");
        }

        #endregion

        #region Content products

        public async Task<IActionResult> GetContentProducts(FlipbookContentProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return await AccessDeniedDataTablesJson();

            var model = await _flipbookModelFactory.PrepareFlipbookContentProductListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ContentProductEdit(FlipbookContentProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return await AccessDeniedDataTablesJson();

            var contentProduct = await _flipbookService.GetFlipbookContentProductByIdAsync(model.Id)
                ?? throw new ArgumentException("No content product found with the specified id");

            contentProduct.DisplayOrder = model.DisplayOrder;
            await _flipbookService.UpdateFlipbookContentProductAsync(contentProduct);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ContentProductDelete(FlipbookContentProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return await AccessDeniedDataTablesJson();

            var contentProduct = await _flipbookService.GetFlipbookContentProductByIdAsync(model.Id)
                ?? throw new ArgumentException("No content product found with the specified id");

            await _flipbookService.DeleteFlipbookContentProductAsync(contentProduct);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductAddPopup(int flipbookContentId)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            //prepare model
            var model = await _flipbookModelFactory.PrepareAddProductToFlipbookContentSearchModelAsync(new AddProductToFlipbookContentSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToFlipbookContentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _flipbookModelFactory.PrepareAddProductToFlipbookContentListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToFlipbookContentModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FlipbookPermissionProvider.ManageFlipbooks))
                return AccessDeniedView();

            //try to get a discount with the specified id
            var content = await _flipbookService.GetFlipbookContentByIdAsync(model.FlipbookContentId)
                ?? throw new ArgumentException("No content found with the specified id");

            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
                    if (await _flipbookService.GetFlipbookContentProductAsync(content.Id, product.Id) is null)
                        await _flipbookService.InsertFlipbookContentProductAsync(new FlipbookContentProduct { ProductId = product.Id, FlipbookContentId = model.FlipbookContentId });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToFlipbookContentSearchModel());
        }

        #endregion

        #endregion
    }
}
