using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;
using NopStation.Plugin.Widgets.SmartCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Controllers;

public partial class SmartCarouselController : BaseWidgetAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ISmartCarouselModelFactory _carouselModelFactory;
    private readonly IAclService _aclService;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISmartCarouselService _carouselService;
    private readonly ISettingService _settingService;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly IWorkContext _workContext;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IWidgetZoneModelFactory _widgetZoneModelFactory;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;
    private readonly SmartCarouselSettings _carouselSetting;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public SmartCarouselController(IStoreContext storeContext,
        ILocalizedEntityService localizedEntityService,
        ISmartCarouselModelFactory carouselModelFactory,
        IAclService aclService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISmartCarouselService carouselService,
        ISettingService settingService,
        IProductService productService,
        IStoreService storeService,
        IWorkContext workContext,
        IConditionModelFactory conditionModelFactory,
        IScheduleModelFactory scheduleModelFactory,
        IWidgetZoneModelFactory widgetZoneModelFactory,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService,
        SmartCarouselSettings carouselSetting,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IVendorService vendorService,
        IPictureService pictureService)
    {
        _storeContext = storeContext;
        _localizedEntityService = localizedEntityService;
        _carouselModelFactory = carouselModelFactory;
        _aclService = aclService;
        _customerService = customerService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _carouselService = carouselService;
        _settingService = settingService;
        _productService = productService;
        _storeService = storeService;
        _workContext = workContext;
        _conditionModelFactory = conditionModelFactory;
        _scheduleModelFactory = scheduleModelFactory;
        _widgetZoneModelFactory = widgetZoneModelFactory;
        _storeMappingService = storeMappingService;
        _permissionService = permissionService;
        _carouselSetting = carouselSetting;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _vendorService = vendorService;
        _pictureService = pictureService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(SmartCarousel carousel, SmartCarouselModel model)
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

    protected virtual async Task SaveCarouselAclAsync(SmartCarousel carousel, SmartCarouselModel model)
    {
        carousel.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _carouselService.UpdateCarouselAsync(carousel);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(carousel);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(carousel, customerRole.Id);
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

    protected virtual async Task UpdateLocalesAsync(SmartCarousel carousel, SmartCarouselModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(carousel,
                     x => x.Title,
                     localized.Title,
                     localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(carousel,
                     x => x.CustomUrl,
                     localized.CustomUrl,
                     localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(SmartCarouselPictureMapping pictureMapping, SmartCarouselPictureModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(pictureMapping,
                     x => x.Label,
                     localized.Label,
                     localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(pictureMapping,
                     x => x.RedirectUrl,
                     localized.RedirectUrl,
                     localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    #region Configure

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var model = await _carouselModelFactory.PrepareConfigurationModelAsync();

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var carouselSettings = await _settingService.LoadSettingAsync<SmartCarouselSettings>(storeScope);

        carouselSettings = model.ToSettings(carouselSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(carouselSettings, x => x.EnableCarousel, model.EnableCarousel_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(carouselSettings, x => x.EnableAjaxLoad, model.EnableAjaxLoad_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var model = await _carouselModelFactory.PrepareCarouselSearchModelAsync(new SmartCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(SmartCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        var model = await _carouselModelFactory.PrepareCarouselListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Create/update/delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var model = await _carouselModelFactory.PrepareCarouselModelAsync(new SmartCarouselModel(), null);
        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async virtual Task<IActionResult> Create(SmartCarouselModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var carousel = model.ToEntity<SmartCarousel>();
            carousel.CreatedOnUtc = DateTime.UtcNow;
            carousel.UpdatedOnUtc = DateTime.UtcNow;

            await _carouselService.InsertCarouselAsync(carousel);

            await UpdateLocalesAsync(carousel, model);

            await SaveStoreMappingsAsync(carousel, model);

            //ACL (customer roles)
            await SaveCarouselAclAsync(carousel, model);

            await _carouselService.UpdateCarouselAsync(carousel);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, carousel, _carouselService.UpdateCarouselAsync);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Created"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = carousel.Id }) :
                RedirectToAction("List");
        }

        model = await _carouselModelFactory.PrepareCarouselModelAsync(model, null);

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(id);
        if (carousel == null || carousel.Deleted)
            return RedirectToAction("List");

        var model = await _carouselModelFactory.PrepareCarouselModelAsync(null, carousel);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async virtual Task<IActionResult> Edit(SmartCarouselModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
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

            //ACL (customer roles)
            await SaveCarouselAclAsync(carousel, model);

            await _carouselService.UpdateCarouselAsync(carousel);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, carousel, _carouselService.UpdateCarouselAsync);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Updated"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = model.Id }) :
                RedirectToAction("List");
        }

        model = await _carouselModelFactory.PrepareCarouselModelAsync(model, carousel);

        return View(model);
    }

    [EditAccess, HttpPost]
    public async virtual Task<IActionResult> Delete(SmartCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(model.Id);
        if (carousel == null || carousel.Deleted)
            return RedirectToAction("List");

        await _carouselService.DeleteCarouselAsync(carousel);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Applied to products

    [HttpPost]
    public virtual async Task<IActionResult> ProductList(SmartCarouselProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _carouselModelFactory.PrepareCarouselProductListModelAsync(searchModel, carousel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductUpdate(SmartCarouselProductModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a related product with the specified id
        var carouselProductMapping = await _carouselService.GetCarouselProductMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No product mapping found with the specified id");

        carouselProductMapping.DisplayOrder = model.DisplayOrder;
        await _carouselService.UpdateCarouselProductMappingAsync(carouselProductMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //remove carousel
        if (await _carouselService.GetCarouselProductMappingByIdAsync(id) is SmartCarouselProductMapping carouselProductMapping)
            await _carouselService.DeleteCarouselProductMappingAsync(carouselProductMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductAddPopup(int carouselId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddProductToCarouselSearchModelAsync(new AddProductToCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList(AddProductToCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddProductToCarouselListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductAddPopup(AddProductToCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(model.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            foreach (var product in selectedProducts)
            {
                if (await _carouselService.GetCarouselProductMappingAsync(carousel.Id, product.Id) is null)
                    await _carouselService.InsertCarouselProductMappingAsync(new SmartCarouselProductMapping { ProductId = product.Id, CarouselId = carousel.Id, DisplayOrder = 1 });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddProductToCarouselSearchModel());
    }

    #endregion

    #region Applied to categories

    [HttpPost]
    public virtual async Task<IActionResult> CategoryList(SmartCarouselCategorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _carouselModelFactory.PrepareCarouselCategoryListModelAsync(searchModel, carousel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryUpdate(SmartCarouselCategoryModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a related category with the specified id
        var carouselCategoryMapping = await _carouselService.GetCarouselCategoryMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No category mapping found with the specified id");

        carouselCategoryMapping.DisplayOrder = model.DisplayOrder;
        await _carouselService.UpdateCarouselCategoryMappingAsync(carouselCategoryMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CategoryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //remove carousel
        if (await _carouselService.GetCarouselCategoryMappingByIdAsync(id) is SmartCarouselCategoryMapping carouselCategoryMapping)
            await _carouselService.DeleteCarouselCategoryMappingAsync(carouselCategoryMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CategoryAddPopup(int carouselId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddCategoryToCarouselSearchModelAsync(new AddCategoryToCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryAddPopupList(AddCategoryToCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddCategoryToCarouselListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CategoryAddPopup(AddCategoryToCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(model.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        var selectedCategorys = await _categoryService.GetCategoriesByIdsAsync(model.SelectedCategoryIds.ToArray());
        if (selectedCategorys.Any())
        {
            foreach (var category in selectedCategorys)
            {
                if (await _carouselService.GetCarouselCategoryMappingAsync(carousel.Id, category.Id) is null)
                    await _carouselService.InsertCarouselCategoryMappingAsync(new SmartCarouselCategoryMapping { CategoryId = category.Id, CarouselId = carousel.Id, DisplayOrder = 1 });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddCategoryToCarouselSearchModel());
    }

    #endregion

    #region Applied to manufacturers

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerList(SmartCarouselManufacturerSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _carouselModelFactory.PrepareCarouselManufacturerListModelAsync(searchModel, carousel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerUpdate(SmartCarouselManufacturerModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a related manufacturer with the specified id
        var carouselManufacturerMapping = await _carouselService.GetCarouselManufacturerMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No manufacturer mapping found with the specified id");

        carouselManufacturerMapping.DisplayOrder = model.DisplayOrder;
        await _carouselService.UpdateCarouselManufacturerMappingAsync(carouselManufacturerMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ManufacturerDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //remove carousel
        if (await _carouselService.GetCarouselManufacturerMappingByIdAsync(id) is SmartCarouselManufacturerMapping carouselManufacturerMapping)
            await _carouselService.DeleteCarouselManufacturerMappingAsync(carouselManufacturerMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ManufacturerAddPopup(int carouselId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddManufacturerToCarouselSearchModelAsync(new AddManufacturerToCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerAddPopupList(AddManufacturerToCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddManufacturerToCarouselListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ManufacturerAddPopup(AddManufacturerToCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(model.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        var selectedManufacturers = await _manufacturerService.GetManufacturersByIdsAsync(model.SelectedManufacturerIds.ToArray());
        if (selectedManufacturers.Any())
        {
            foreach (var manufacturer in selectedManufacturers)
            {
                if (await _carouselService.GetCarouselManufacturerMappingAsync(carousel.Id, manufacturer.Id) is null)
                    await _carouselService.InsertCarouselManufacturerMappingAsync(new SmartCarouselManufacturerMapping { ManufacturerId = manufacturer.Id, CarouselId = carousel.Id, DisplayOrder = 1 });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddManufacturerToCarouselSearchModel());
    }

    #endregion

    #region Applied to vendors

    [HttpPost]
    public virtual async Task<IActionResult> VendorList(SmartCarouselVendorSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _carouselModelFactory.PrepareCarouselVendorListModelAsync(searchModel, carousel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> VendorUpdate(SmartCarouselVendorModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a related vendor with the specified id
        var carouselVendorMapping = await _carouselService.GetCarouselVendorMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No vendor mapping found with the specified id");

        carouselVendorMapping.DisplayOrder = model.DisplayOrder;
        await _carouselService.UpdateCarouselVendorMappingAsync(carouselVendorMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> VendorDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //remove carousel
        if (await _carouselService.GetCarouselVendorMappingByIdAsync(id) is SmartCarouselVendorMapping carouselVendorMapping)
            await _carouselService.DeleteCarouselVendorMappingAsync(carouselVendorMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> VendorAddPopup(int carouselId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddVendorToCarouselSearchModelAsync(new AddVendorToCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> VendorAddPopupList(AddVendorToCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddVendorToCarouselListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> VendorAddPopup(AddVendorToCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(model.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        var selectedVendors = await _carouselService.GetVendorsByIdsAsync(model.SelectedVendorIds.ToArray());
        if (selectedVendors.Any())
        {
            foreach (var vendor in selectedVendors)
            {
                if (await _carouselService.GetCarouselVendorMappingAsync(carousel.Id, vendor.Id) is null)
                    await _carouselService.InsertCarouselVendorMappingAsync(new SmartCarouselVendorMapping { VendorId = vendor.Id, CarouselId = carousel.Id, DisplayOrder = 1 });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddVendorToCarouselSearchModel());
    }

    #endregion

    #region Carousel pictures

    public virtual async Task<IActionResult> PictureAddPopup(int carouselId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(carouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        var model = await _carouselModelFactory.PrepareCarouselPictureModelAsync(new SmartCarouselPictureModel(), null, carousel);

        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> PictureAddPopup(SmartCarouselPictureModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(model.CarouselId);
        if (carousel == null || carousel.Deleted)
            return RedirectToAction("List");

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(model.PictureId);
        if (picture == null)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture.Required"));

        if (ModelState.IsValid)
        {
            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            var pictureMapping = model.ToEntity<SmartCarouselPictureMapping>();

            await _carouselService.InsertCarouselPictureMappingAsync(pictureMapping);

            await UpdateLocalesAsync(pictureMapping, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        model = await _carouselModelFactory.PrepareCarouselPictureModelAsync(model, null, carousel);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PictureList(SmartCarouselPictureSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _carouselModelFactory.PrepareCarouselPictureListModelAsync(searchModel, carousel);

        return Json(model);
    }

    public virtual async Task<IActionResult> PictureEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel picture with the specified id
        var pictureMapping = await _carouselService.GetCarouselPictureMappingByIdAsync(id);
        if (pictureMapping == null)
            return RedirectToAction("List");

        var carousel = await _carouselService.GetCarouselByIdAsync(pictureMapping.CarouselId);
        if (carousel == null || carousel.Deleted)
            return RedirectToAction("List");

        var model = await _carouselModelFactory.PrepareCarouselPictureModelAsync(null, pictureMapping, carousel);

        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> PictureEditPopup(SmartCarouselPictureModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(model.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //try to get a carousel picture with the specified id
        var pictureMapping = await _carouselService.GetCarouselPictureMappingByIdAsync(model.Id)
            ?? throw new ArgumentException("No carousel picture mapping found with the specified id");

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(model.PictureId);
        if (picture == null)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture.Required"));

        if (ModelState.IsValid)
        {
            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            pictureMapping = model.ToEntity(pictureMapping);

            await _carouselService.UpdateCarouselPictureMappingAsync(pictureMapping);

            await UpdateLocalesAsync(pictureMapping, model);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        model = await _carouselModelFactory.PrepareCarouselPictureModelAsync(model, pictureMapping, carousel);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PictureDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel picture with the specified id
        var carouselPicture = await _carouselService.GetCarouselPictureMappingByIdAsync(id)
            ?? throw new ArgumentException("No carousel picture found with the specified id");

        var pictureId = carouselPicture.PictureId;
        await _carouselService.DeleteCarouselPictureMappingAsync(carouselPicture);

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(pictureId)
            ?? throw new ArgumentException("No picture found with the specified id");

        await _pictureService.DeletePictureAsync(picture);

        return new NullJsonResult();
    }

    #endregion

    #region Widget zone mappings

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneList(WidgetZoneSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        var model = await base.WidgetZoneListAsync(searchModel, carousel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneCreate(WidgetZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        var carousel = await _carouselService.GetCarouselByIdAsync(model.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.WidgetZoneCreateAsync(model, carousel);

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneEdit(WidgetZoneModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        var carousel = await _carouselService.GetCarouselByIdAsync(model.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.WidgetZoneEditAsync(model, carousel);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        var carousel = await _carouselService.GetCarouselByIdAsync(entityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.WidgetZoneDeleteAsync(id, carousel);

        return new NullJsonResult();
    }

    #endregion

    #region Customer condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionList(CustomerConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await base.CustomerConditionListAsync(searchModel, carousel);

        return Json(model);
    }

    public virtual async Task<IActionResult> CustomerConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(entityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.CustomerConditionDeleteAsync(id, carousel);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CustomerConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(entityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionSearchModelAsync(new AddCustomerToConditionSearchModel(), carousel);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerConditionAddList(AddCustomerToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CustomerConditionAddPopup(AddCustomerToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(model.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.CustomerConditionAddAsync(model, carousel);

        ViewBag.RefreshPage = true;

        return View(new AddCustomerToConditionSearchModel());
    }

    #endregion

    #region Product condition mappings

    [HttpPost]
    public virtual async Task<IActionResult> ProductConditionList(ProductConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await base.ProductConditionListAsync(searchModel, carousel);

        return Json(model);
    }

    public virtual async Task<IActionResult> ProductConditionDelete(int id, int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(entityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.ProductConditionDeleteAsync(id, carousel);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductConditionAddPopup(int entityId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(entityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionSearchModelAsync(new AddProductToConditionSearchModel(), carousel);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductConditionAddList(AddProductToConditionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductConditionAddPopup(AddProductToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
            return AccessDeniedView();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(model.EntityId);
        if (carousel == null || carousel.Deleted)
            throw new ArgumentException("No carousel found with the specified id");

        await base.ProductConditionAddAsync(model, carousel);

        ViewBag.RefreshPage = true;

        return View(new AddProductToConditionSearchModel());
    }

    #endregion

    #endregion
}
