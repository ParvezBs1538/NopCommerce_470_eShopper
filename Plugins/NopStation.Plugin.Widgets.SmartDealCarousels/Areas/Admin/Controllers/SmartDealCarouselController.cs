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
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;
using NopStation.Plugin.Widgets.SmartDealCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Controllers;

public partial class SmartDealCarouselController : BaseWidgetAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ISmartDealCarouselModelFactory _carouselModelFactory;
    private readonly IAclService _aclService;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISmartDealCarouselService _carouselService;
    private readonly ISettingService _settingService;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly IWorkContext _workContext;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IWidgetZoneModelFactory _widgetZoneModelFactory;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPermissionService _permissionService;
    private readonly SmartDealCarouselSettings _carouselSetting;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public SmartDealCarouselController(IStoreContext storeContext,
        ILocalizedEntityService localizedEntityService,
        ISmartDealCarouselModelFactory carouselModelFactory,
        IAclService aclService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISmartDealCarouselService carouselService,
        ISettingService settingService,
        IProductService productService,
        IStoreService storeService,
        IWorkContext workContext,
        IConditionModelFactory conditionModelFactory,
        IScheduleModelFactory scheduleModelFactory,
        IWidgetZoneModelFactory widgetZoneModelFactory,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService,
        SmartDealCarouselSettings carouselSetting,
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

    protected virtual async Task SaveStoreMappingsAsync(SmartDealCarousel carousel, SmartDealCarouselModel model)
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

    protected virtual async Task SaveCarouselAclAsync(SmartDealCarousel carousel, SmartDealCarouselModel model)
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

    protected virtual async Task UpdateLocalesAsync(SmartDealCarousel carousel, SmartDealCarouselModel model)
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

    #endregion

    #region Methods

    #region Configure

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        var model = await _carouselModelFactory.PrepareConfigurationModelAsync();

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var carouselSettings = await _settingService.LoadSettingAsync<SmartDealCarouselSettings>(storeScope);

        carouselSettings = model.ToSettings(carouselSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(carouselSettings, x => x.EnableCarousel, model.EnableCarousel_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(carouselSettings, x => x.EnableAjaxLoad, model.EnableAjaxLoad_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(carouselSettings, x => x.CarouselPictureSize, model.CarouselPictureSize_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        var model = await _carouselModelFactory.PrepareCarouselSearchModelAsync(new SmartDealCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(SmartDealCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return await AccessDeniedDataTablesJson();

        var model = await _carouselModelFactory.PrepareCarouselListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Create/update/delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        var model = await _carouselModelFactory.PrepareCarouselModelAsync(new SmartDealCarouselModel(), null);
        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(SmartDealCarouselModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var carousel = model.ToEntity<SmartDealCarousel>();
            carousel.CreatedOnUtc = DateTime.UtcNow;
            carousel.UpdatedOnUtc = DateTime.UtcNow;

            await _carouselService.InsertCarouselAsync(carousel);

            await UpdateLocalesAsync(carousel, model);

            await SaveStoreMappingsAsync(carousel, model);

            //ACL (customer roles)
            await SaveCarouselAclAsync(carousel, model);

            await _carouselService.UpdateCarouselAsync(carousel);

            //update schedule mappings
            await UpdateScheduleMappingsAsync(model.Schedule, carousel, async (carousel) => await _carouselService.UpdateCarouselAsync(carousel));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Created"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = carousel.Id }) :
                RedirectToAction("List");
        }

        model = await _carouselModelFactory.PrepareCarouselModelAsync(model, null);

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(id);
        if (carousel == null || carousel.Deleted)
            return RedirectToAction("List");

        var model = await _carouselModelFactory.PrepareCarouselModelAsync(null, carousel);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(SmartDealCarouselModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
            await UpdateScheduleMappingsAsync(model.Schedule, carousel, async (carousel) => await _carouselService.UpdateCarouselAsync(carousel));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Updated"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = model.Id }) :
                RedirectToAction("List");
        }

        model = await _carouselModelFactory.PrepareCarouselModelAsync(model, carousel);

        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> Delete(SmartDealCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        var carousel = await _carouselService.GetCarouselByIdAsync(model.Id);
        if (carousel == null || carousel.Deleted)
            return RedirectToAction("List");

        await _carouselService.DeleteCarouselAsync(carousel);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Applied to products

    [HttpPost]
    public virtual async Task<IActionResult> ProductList(SmartDealCarouselProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return await AccessDeniedDataTablesJson();

        //try to get a carousel with the specified id
        var carousel = await _carouselService.GetCarouselByIdAsync(searchModel.CarouselId)
            ?? throw new ArgumentException("No carousel found with the specified id");

        //prepare model
        var model = await _carouselModelFactory.PrepareCarouselProductListModelAsync(searchModel, carousel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductUpdate(SmartDealCarouselProductModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        //remove carousel
        if (await _carouselService.GetCarouselProductMappingByIdAsync(id) is SmartDealCarouselProductMapping carouselProductMapping)
            await _carouselService.DeleteCarouselProductMappingAsync(carouselProductMapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductAddPopup(int carouselId)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return AccessDeniedView();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddProductToCarouselSearchModelAsync(new AddProductToCarouselSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList(AddProductToCarouselSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _carouselModelFactory.PrepareAddProductToCarouselListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductAddPopup(AddProductToCarouselModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
                    await _carouselService.InsertCarouselProductMappingAsync(new SmartDealCarouselProductMapping { ProductId = product.Id, CarouselId = carousel.Id, DisplayOrder = 1 });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddProductToCarouselSearchModel());
    }

    #endregion

    #region Widget zone mappings

    [HttpPost]
    public virtual async Task<IActionResult> WidgetZoneList(WidgetZoneSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddCustomerToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CustomerConditionAddPopup(AddCustomerToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _conditionModelFactory.PrepareAddProductToConditionListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductConditionAddPopup(AddProductToConditionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
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
