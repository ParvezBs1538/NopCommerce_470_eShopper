using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Controllers;

public class QuoteCartController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ICategoryModelFactory _categoryModelFactory;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly INotificationService _notificationService;
    private readonly IManufacturerModelFactory _manufacturerModelFactory;
    private readonly IPermissionService _permissionService;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IQuoteCartService _quoteCartService;
    private readonly IQuoteRequestWhitelistService _quoteRequestWhitelistService;
    private readonly IQuoteWhitelistModelFactory _quoteWhitelistModelFactory;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IVendorModelFactory _vendorModelFactory;

    #endregion

    #region Ctor

    public QuoteCartController(
        ILocalizationService localizationService,
        ICategoryModelFactory categoryModelFactory,
        IAclSupportedModelFactory aclSupportedModelFactory,
        INotificationService notificationService,
        IManufacturerModelFactory manufacturerModelFactory,
        IPermissionService permissionService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IQuoteCartService quoteCartService,
        IQuoteRequestWhitelistService quoteRequestWhitelistService,
        IQuoteWhitelistModelFactory quoteWhitelistModelFactory,
        ISettingService settingService,
        IStoreContext storeContext,
        IVendorModelFactory vendorModelFactory)
    {
        _localizationService = localizationService;
        _categoryModelFactory = categoryModelFactory;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _notificationService = notificationService;
        _manufacturerModelFactory = manufacturerModelFactory;
        _permissionService = permissionService;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _quoteCartService = quoteCartService;
        _quoteRequestWhitelistService = quoteRequestWhitelistService;
        _quoteWhitelistModelFactory = quoteWhitelistModelFactory;
        _settingService = settingService;
        _storeContext = storeContext;
        _vendorModelFactory = vendorModelFactory;
    }

    #endregion

    #region Methods

    #region Configure 

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var quoteCartSettings = await _settingService.LoadSettingAsync<QuoteCartSettings>(storeId);
        var model = quoteCartSettings.ToSettingsModel<ConfigurationModel>();

        model.SelectedCustomerRoleIds = await _quoteCartService.GetAllowedCustomerRoleIdsAsync();
        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);
        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return View(model);

        model.EnableQuoteCart_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.EnableQuoteCart, storeId);
        model.EnableWhitelist_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.EnableWhitelist, storeId);
        model.MaxQuoteItemCount_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.MaxQuoteItemCount, storeId);
        model.WhitelistAllManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.SubjectToAcl, storeId);
        model.WhitelistAllProducts_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.WhitelistAllProducts, storeId);
        model.WhitelistAllCategories_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.WhitelistAllCategories, storeId);
        model.WhitelistAllManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.WhitelistAllManufacturers, storeId);
        model.WhitelistAllVendors_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.WhitelistAllVendors, storeId);
        model.ClearCartAfterSubmission_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.ClearCartAfterSubmission, storeId);
        model.CustomerCanEnterPrice_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.CustomerCanEnterPrice, storeId);
        model.CustomerCanCancelQuote_OverrideForStore = await _settingService.SettingExistsAsync(quoteCartSettings, x => x.CustomerCanCancelQuote, storeId);

        return View(model);
    }

    [EditAccess, HttpPost]
    [FormValueRequired("save")]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var quoteCartSettings = await _settingService.LoadSettingAsync<QuoteCartSettings>(storeScope);
        quoteCartSettings = model.ToSettings(quoteCartSettings);

        // permissions
        var permissionRecord = await _quoteCartService.GetSendQuotePermissionRecordAsync();
        var oldRoleIds = await _quoteCartService.GetAllowedCustomerRoleIdsAsync();
        var roleIdsToAdd = model.SelectedCustomerRoleIds.Except(oldRoleIds);
        var roleIdsToRemove = oldRoleIds.Except(model.SelectedCustomerRoleIds);

        if (permissionRecord == null)
        {
            permissionRecord = QuoteCartPermissionProvider.SendQuoteRequest;
            await _permissionService.InsertPermissionRecordAsync(permissionRecord);
        }

        foreach (var roleId in roleIdsToAdd)
        {
            await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new()
            {
                CustomerRoleId = roleId,
                PermissionRecordId = permissionRecord.Id
            });
        }

        foreach (var roleId in roleIdsToRemove)
        {
            await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(permissionRecord.Id, roleId);
        }

        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.EnableQuoteCart, model.EnableQuoteCart_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.MaxQuoteItemCount, model.MaxQuoteItemCount_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.WhitelistAllVendors, model.WhitelistAllVendors_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.WhitelistAllCategories, model.WhitelistAllCategories_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.WhitelistAllManufacturers, model.WhitelistAllManufacturers_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.WhitelistAllProducts, model.WhitelistAllProducts_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.EnableWhitelist, model.EnableWhitelist_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.ClearCartAfterSubmission, model.ClearCartAfterSubmission_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.CustomerCanCancelQuote, model.CustomerCanCancelQuote_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(quoteCartSettings, x => x.CustomerCanEnterPrice, model.CustomerCanEnterPrice_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Updated"));

        return RedirectToAction(nameof(Configure));
    }

    #endregion

    #region Whitelist products

    public async Task<IActionResult> ProductList(ProductSearchModel productSearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        var products = await _quoteRequestWhitelistService
            .GetWhitelistedProductsAsync(productSearchModel.Page - 1, productSearchModel.PageSize);

        var model = await new ProductListModel().PrepareToGridAsync(productSearchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                //fill in model values from the entity
                return await Task.FromResult(product.ToModel<ProductModel>());
            });
        });
        return Json(model);
    }

    public virtual async Task<IActionResult> ProductAddPopup()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

        return View(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ProductAddPopup(AddEntityToWhitelistModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        if (model.SelectedEntityIds.Any())
        {
            await _quoteRequestWhitelistService.AddPermissionsAsync<Product>(model.SelectedEntityIds);
        }

        ViewBag.RefreshPage = true;

        return View(new ProductSearchModel());
    }

    public virtual async Task<IActionResult> ProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        await _quoteRequestWhitelistService.RemovePermissionAsync<Product>(id);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList(ProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _quoteWhitelistModelFactory.PrepareAddProductListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region Whitelist categories

    public async Task<IActionResult> CategoryList(CategorySearchModel categorySearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        var categories = await _quoteRequestWhitelistService.GetWhitelistedCategoriesAsync(categorySearchModel.Page - 1, categorySearchModel.PageSize);

        var model = await new CategoryListModel().PrepareToGridAsync(categorySearchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                //fill in model values from the entity
                var categoryModel = category.ToModel<CategoryModel>();

                return await Task.FromResult(categoryModel);
            });
        });
        return Json(model);
    }

    public virtual async Task<IActionResult> CategoryAddPopup()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //prepare model
        var model = await _categoryModelFactory.PrepareCategorySearchModelAsync(new CategorySearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryAddPopupList(CategorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _quoteWhitelistModelFactory.PrepareCategoryListModelAsync(searchModel);

        return Json(model);
    }

    [EditAccess, HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> CategoryAddPopup(AddEntityToWhitelistModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        if (model.SelectedEntityIds.Any())
        {
            await _quoteRequestWhitelistService.AddPermissionsAsync<Category>(model.SelectedEntityIds);
        }

        ViewBag.RefreshPage = true;

        return View(new CategorySearchModel());
    }

    [EditAccessAjax]
    public virtual async Task<IActionResult> CategoryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        await _quoteRequestWhitelistService.RemovePermissionAsync<Category>(id);

        return new NullJsonResult();
    }

    #endregion

    #region Whitelist meanufacturers

    public async Task<IActionResult> ManufacturerList(ManufacturerSearchModel manufacturerSearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        var manufacturers = await _quoteRequestWhitelistService.GetWhitelistedManufacturersAsync(manufacturerSearchModel.Page - 1, manufacturerSearchModel.PageSize);

        var model = await new ManufacturerListModel().PrepareToGridAsync(manufacturerSearchModel, manufacturers, () =>
        {
            return manufacturers.SelectAwait(async manufacturer =>
            {
                //fill in model values from the entity
                var manufacturerModel = manufacturer.ToModel<ManufacturerModel>();

                return await Task.FromResult(manufacturerModel);
            });
        });
        return Json(model);
    }

    public virtual async Task<IActionResult> ManufacturerAddPopup()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //prepare model
        var model = await _manufacturerModelFactory.PrepareManufacturerSearchModelAsync(new ManufacturerSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerAddPopupList(ManufacturerSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _quoteWhitelistModelFactory.PrepareManufacturerListModelAsync(searchModel);

        return Json(model);
    }

    [EditAccess, HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> ManufacturerAddPopup(AddEntityToWhitelistModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        if (model.SelectedEntityIds.Any())
        {
            await _quoteRequestWhitelistService.AddPermissionsAsync<Manufacturer>(model.SelectedEntityIds);
        }

        ViewBag.RefreshPage = true;

        return View(new ManufacturerSearchModel());
    }

    [EditAccessAjax]
    public virtual async Task<IActionResult> ManufacturerDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        await _quoteRequestWhitelistService.RemovePermissionAsync<Manufacturer>(id);

        return new NullJsonResult();
    }

    #endregion

    #region Whitelist vendors

    public async Task<IActionResult> VendorList(VendorSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        var vendors = await _quoteRequestWhitelistService.GetWhitelistedVendorsAsync(searchModel.Page - 1, searchModel.PageSize);

        var model = await new VendorListModel().PrepareToGridAsync(searchModel, vendors, () =>
        {
            return vendors.SelectAwait(async vendor =>
            {
                //fill in model values from the entity
                return await Task.FromResult(vendor.ToModel<VendorModel>());
            });
        });
        return Json(model);
    }

    public virtual async Task<IActionResult> VendorAddPopup()
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //prepare model
        var model = await _vendorModelFactory.PrepareVendorSearchModelAsync(new());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> VendorAddPopupList(VendorSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _vendorModelFactory.PrepareVendorListModelAsync(searchModel);

        return Json(model);
    }

    [EditAccess, HttpPost]
    [FormValueRequired("save")]
    public virtual async Task<IActionResult> VendorAddPopup(AddEntityToWhitelistModel model)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        if (model.SelectedEntityIds.Any())
        {
            await _quoteRequestWhitelistService.AddPermissionsAsync<Vendor>(model.SelectedEntityIds);
        }

        ViewBag.RefreshPage = true;

        return View(new VendorSearchModel());
    }

    [EditAccessAjax]
    public virtual async Task<IActionResult> VendorDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration))
            return await AccessDeniedDataTablesJson();

        await _quoteRequestWhitelistService.RemovePermissionAsync<Vendor>(id);

        return new NullJsonResult();
    }

    #endregion

    #endregion
}
