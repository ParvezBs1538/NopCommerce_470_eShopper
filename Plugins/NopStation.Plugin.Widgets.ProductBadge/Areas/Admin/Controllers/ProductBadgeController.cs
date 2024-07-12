using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductBadge.Domains;
using NopStation.Plugin.Widgets.ProductBadge.Services;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Controllers;

public partial class ProductBadgeController : NopStationAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IBadgeModelFactory _badgeModelFactory;
    private readonly IBadgeService _badgeService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;
    private readonly IAclService _aclService;
    private readonly ICustomerService _customerService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public ProductBadgeController(IPermissionService permissionService,
        IStoreContext storeContext,
        ISettingService settingService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IBadgeModelFactory badgeModelFactory,
        IBadgeService badgeService,
        ILocalizedEntityService localizedEntityService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IProductService productService,
        IVendorService vendorService,
        IAclService aclService,
        ICustomerService customerService,
        IBaseAdminModelFactory baseAdminModelFactory,
        IStaticCacheManager staticCacheManager,
        IPictureService pictureService)
    {
        _permissionService = permissionService;
        _storeContext = storeContext;
        _settingService = settingService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _badgeModelFactory = badgeModelFactory;
        _badgeService = badgeService;
        _localizedEntityService = localizedEntityService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _productService = productService;
        _vendorService = vendorService;
        _aclService = aclService;
        _customerService = customerService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _staticCacheManager = staticCacheManager;
        _pictureService = pictureService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Badge badge, BadgeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(badge,
                     x => x.Text,
                     localized.Text,
                     localized.LanguageId);
        }
    }

    protected virtual async Task SaveStoreMappingsAsync(Badge badge, BadgeModel model)
    {
        badge.LimitedToStores = model.SelectedStoreIds.Any();

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(badge);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    await _storeMappingService.InsertStoreMappingAsync(badge, store.Id);
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

    protected virtual async Task SaveBadgeAclAsync(Badge badge, BadgeModel model)
    {
        badge.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _badgeService.UpdateBadgeAsync(badge);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(badge);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(badge, customerRole.Id);
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

    #endregion

    #region Methods

    #region Configure

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var model = await _badgeModelFactory.PrepareConfigurationModelAsync();
        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var badgeSettings = await _settingService.LoadSettingAsync<ProductBadgeSettings>(storeScope);
        badgeSettings = model.ToSettings(badgeSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.EnableAjaxLoad, model.EnableAjaxLoad_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingAsync(badgeSettings, x => x.CacheActiveBadges, 0, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.ProductBoxWidgetZone, model.ProductBoxWidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.ProductDetailsWidgetZone, model.ProductDetailsWidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.SmallBadgeWidth, model.SmallBadgeWidth_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.MediumBadgeWidth, model.MediumBadgeWidth_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.LargeBadgeWidth, model.LargeBadgeWidth_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(badgeSettings, x => x.IncreaseWidthInDetailsPageByPercentage, model.IncreaseWidthInDetailsPageByPercentage_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region List

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var model = await _badgeModelFactory.PrepareBadgeSearchModelAsync(new BadgeSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> List(BadgeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        var model = await _badgeModelFactory.PrepareBadgeListModelAsync(searchModel);
        return Json(model);
    }

    #endregion

    #region Create / Update / Delete

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var model = await _badgeModelFactory.PrepareBadgeModelAsync(new BadgeModel(), null);
        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(BadgeModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var badge = model.ToEntity<Badge>();
            badge.BestSellOrderStatusIds = model.BestSellOrderStatusIds.ToFormattedString();
            badge.BestSellPaymentStatusIds = model.BestSellPaymentStatusIds.ToFormattedString();
            badge.BestSellShippingStatusIds = model.BestSellShippingStatusIds.ToFormattedString();

            await _badgeService.InsertBadgeAsync(badge);

            await UpdateLocalesAsync(badge, model);

            await SaveStoreMappingsAsync(badge, model);
            await SaveBadgeAclAsync(badge, model);

            await _badgeService.UpdateBadgeAsync(badge);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.Created"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = badge.Id }) :
                RedirectToAction("List");
        }

        model = await _badgeModelFactory.PrepareBadgeModelAsync(model, null);

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var badge = await _badgeService.GetBadgeByIdAsync(id);
        if (badge == null || badge.Deleted)
            return RedirectToAction("List");

        var model = await _badgeModelFactory.PrepareBadgeModelAsync(null, badge);

        return View(model);
    }

    [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(BadgeModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var badge = await _badgeService.GetBadgeByIdAsync(model.Id);
        if (badge == null || badge.Deleted)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            badge = model.ToEntity(badge);
            badge.BestSellOrderStatusIds = model.BestSellOrderStatusIds.ToFormattedString();
            badge.BestSellPaymentStatusIds = model.BestSellPaymentStatusIds.ToFormattedString();
            badge.BestSellShippingStatusIds = model.BestSellShippingStatusIds.ToFormattedString();

            await UpdateLocalesAsync(badge, model);

            await SaveStoreMappingsAsync(badge, model);
            await SaveBadgeAclAsync(badge, model);

            await _badgeService.UpdateBadgeAsync(badge);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.Updated"));

            return continueEditing ?
                RedirectToAction("Edit", new { id = model.Id }) :
                RedirectToAction("List");
        }

        model = await _badgeModelFactory.PrepareBadgeModelAsync(model, badge);
        return View(model);
    }

    [EditAccess, HttpPost]
    public virtual async Task<IActionResult> Delete(BadgeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        var badge = await _badgeService.GetBadgeByIdAsync(model.Id);
        if (badge == null || badge.Deleted)
            return RedirectToAction("List");

        await _badgeService.DeleteBadgeAsync(badge);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Applied to categories

    [HttpPost]
    public virtual async Task<IActionResult> CategoryList(BadgeCategorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(searchModel.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        //prepare model
        var model = await _badgeModelFactory.PrepareBadgeCategoryListModelAsync(searchModel, badge);

        return Json(model);
    }

    public virtual async Task<IActionResult> CategoryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        if (await _badgeService.GetBadgeCategoryMappingByIdAsync(id) is BadgeCategoryMapping mapping)
            await _badgeService.DeleteBadgeCategoryMappingAsync(mapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> CategoryAddPopup(int badgeId)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddCategoryToBadgeSearchModelAsync(new AddCategoryToBadgeSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryAddPopupList(AddCategoryToBadgeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddCategoryToBadgeListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CategoryAddPopup(AddCategoryToBadgeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(model.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        var existingCategoryMappings = await _badgeService.GetBadgeCategoryMappingsAsync(model.BadgeId, false);
        var categories = await _categoryService.GetCategoriesByIdsAsync(model.SelectedCategoryIds.ToArray());

        foreach (var category in categories)
        {
            if (existingCategoryMappings.FirstOrDefault(x => x.CategoryId == category.Id) is null)
                await _badgeService.InsertBadgeCategoryMappingAsync(new BadgeCategoryMapping { BadgeId = badge.Id, CategoryId = category.Id });

            await _badgeService.UpdateBadgeAsync(badge);
        }

        ViewBag.RefreshPage = true;

        return View(new AddCategoryToBadgeSearchModel());
    }

    #endregion

    #region Applied to manufacturers

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerList(BadgeManufacturerSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(searchModel.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        //prepare model
        var model = await _badgeModelFactory.PrepareBadgeManufacturerListModelAsync(searchModel, badge);

        return Json(model);
    }

    public virtual async Task<IActionResult> ManufacturerDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        if (await _badgeService.GetBadgeManufacturerMappingByIdAsync(id) is BadgeManufacturerMapping mapping)
            await _badgeService.DeleteBadgeManufacturerMappingAsync(mapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ManufacturerAddPopup(int badgeId)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddManufacturerToBadgeSearchModelAsync(new AddManufacturerToBadgeSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerAddPopupList(AddManufacturerToBadgeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddManufacturerToBadgeListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ManufacturerAddPopup(AddManufacturerToBadgeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(model.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        var existingManufacturerMappings = await _badgeService.GetBadgeManufacturerMappingsAsync(model.BadgeId, false);
        var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(model.SelectedManufacturerIds.ToArray());

        foreach (var manufacturer in manufacturers)
        {
            if (existingManufacturerMappings.FirstOrDefault(x => x.ManufacturerId == manufacturer.Id) is null)
                await _badgeService.InsertBadgeManufacturerMappingAsync(new BadgeManufacturerMapping { BadgeId = badge.Id, ManufacturerId = manufacturer.Id });

            await _badgeService.UpdateBadgeAsync(badge);
        }

        ViewBag.RefreshPage = true;

        return View(new AddManufacturerToBadgeSearchModel());
    }

    #endregion

    #region Applied to products

    [HttpPost]
    public virtual async Task<IActionResult> ProductList(BadgeProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(searchModel.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        //prepare model
        var model = await _badgeModelFactory.PrepareBadgeProductListModelAsync(searchModel, badge);

        return Json(model);
    }

    public virtual async Task<IActionResult> ProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        if (await _badgeService.GetBadgeProductMappingByIdAsync(id) is BadgeProductMapping mapping)
            await _badgeService.DeleteBadgeProductMappingAsync(mapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> ProductAddPopup(int badgeId)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddProductToBadgeSearchModelAsync(new AddProductToBadgeSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList(AddProductToBadgeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddProductToBadgeListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductAddPopup(AddProductToBadgeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(model.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        var existingProductMappings = await _badgeService.GetBadgeProductMappingsAsync(model.BadgeId);
        var products = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());

        foreach (var product in products)
        {
            if (existingProductMappings.FirstOrDefault(x => x.ProductId == product.Id) is null)
                await _badgeService.InsertBadgeProductMappingAsync(new BadgeProductMapping { BadgeId = badge.Id, ProductId = product.Id });

            await _badgeService.UpdateBadgeAsync(badge);
        }

        ViewBag.RefreshPage = true;

        return View(new AddProductToBadgeSearchModel());
    }

    #endregion

    #region Applied to vendors

    [HttpPost]
    public virtual async Task<IActionResult> VendorList(BadgeVendorSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(searchModel.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        //prepare model
        var model = await _badgeModelFactory.PrepareBadgeVendorListModelAsync(searchModel, badge);

        return Json(model);
    }

    public virtual async Task<IActionResult> VendorDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        if (await _badgeService.GetBadgeVendorMappingByIdAsync(id) is BadgeVendorMapping mapping)
            await _badgeService.DeleteBadgeVendorMappingAsync(mapping);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> VendorAddPopup(int badgeId)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddVendorToBadgeSearchModelAsync(new AddVendorToBadgeSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> VendorAddPopupList(AddVendorToBadgeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _badgeModelFactory.PrepareAddVendorToBadgeListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> VendorAddPopup(AddVendorToBadgeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
            return AccessDeniedView();

        //try to get a badge with the specified id
        var badge = await _badgeService.GetBadgeByIdAsync(model.BadgeId)
            ?? throw new ArgumentException("No badge found with the specified id");

        var existingVendorMappings = await _badgeService.GetBadgeVendorMappingsAsync(model.BadgeId, false);

        foreach (var id in model.SelectedVendorIds)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null || vendor.Deleted)
                continue;

            if (existingVendorMappings.FirstOrDefault(x => x.VendorId == vendor.Id) is null)
                await _badgeService.InsertBadgeVendorMappingAsync(new BadgeVendorMapping { VendorId = vendor.Id, BadgeId = badge.Id });

            await _badgeService.UpdateBadgeAsync(badge);
        }

        ViewBag.RefreshPage = true;

        return View(new AddVendorToBadgeSearchModel());
    }

    #endregion

    #endregion
}