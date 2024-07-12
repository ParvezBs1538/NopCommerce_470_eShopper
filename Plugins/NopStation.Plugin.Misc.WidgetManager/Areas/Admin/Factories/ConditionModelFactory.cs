using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data.Mapping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Services;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;

public class ConditionModelFactory : IConditionModelFactory
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IConditionService _conditionMappingService;
    private readonly ILocalizationService _localizationService;
    private readonly CustomerSettings _customerSettings;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IProductService _productService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IWorkContext _workContext;
    private readonly VendorSettings _vendorSettings;
    private readonly CatalogSettings _catalogSettings;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ICategoryService _categoryService;

    #endregion

    #region Ctor

    public ConditionModelFactory(ICustomerService customerService,
        IConditionService conditionMappingService,
        ILocalizationService localizationService,
        CustomerSettings customerSettings,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IGenericAttributeService genericAttributeService,
        IProductService productService,
        IBaseAdminModelFactory baseAdminModelFactory,
        IWorkContext workContext,
        VendorSettings vendorSettings,
        CatalogSettings catalogSettings,
        IUrlRecordService urlRecordService,
        ICategoryService categoryService)
    {
        _customerService = customerService;
        _conditionMappingService = conditionMappingService;
        _localizationService = localizationService;
        _customerSettings = customerSettings;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _genericAttributeService = genericAttributeService;
        _productService = productService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _workContext = workContext;
        _vendorSettings = vendorSettings;
        _catalogSettings = catalogSettings;
        _urlRecordService = urlRecordService;
        _categoryService = categoryService;
    }

    #endregion

    #region Methods

    #region Customer condition mappings

    public virtual Task PrepareCustomerConditionMappingSearchModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
        where TModel : ICustomerConditionSupportedModel
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (entity != null)
        {
            var searchModel = new CustomerConditionSearchModel();
            searchModel.EntityId = entity.Id;
            searchModel.EntityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

            //prepare page parameters
            searchModel.SetGridPageSize();

            model.CustomerConditionSearchModel = searchModel;
        }

        return Task.CompletedTask;
    }

    public virtual async Task<CustomerConditionListModel> PrepareCustomerConditionMappingListModelAsync(CustomerConditionSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get customer condition mappings
        var customerConditionMappings = (await _conditionMappingService.GetEntityCustomerConditionsAsync(searchModel.EntityId, searchModel.EntityName)).ToPagedList(searchModel);

        //prepare list model
        var model = await new CustomerConditionListModel().PrepareToGridAsync(searchModel, customerConditionMappings, () =>
        {
            return customerConditionMappings.SelectAwait(async ccm =>
            {
                //fill in model values from the entity
                var ccmm = ccm.ToModel<CustomerConditionModel>();

                var customer = await _customerService.GetCustomerByIdAsync(ccm.CustomerId);
                if (customer != null && await _customerService.IsRegisteredAsync(customer))
                    ccmm.CustomerEmail = customer.Email;
                else
                    ccmm.CustomerEmail = await _localizationService.GetResourceAsync("Admin.NopStation.WidgetManager.Common.Guest");

                ccmm.Active = customer.Active;

                return ccmm;
            });
        });

        return model;
    }

    public virtual async Task<AddCustomerToConditionSearchModel> PrepareAddCustomerToConditionSearchModelAsync<TEntity>(AddCustomerToConditionSearchModel searchModel, TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        searchModel.EntityId = entity.Id;
        searchModel.EntityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        searchModel.UsernamesEnabled = _customerSettings.UsernamesEnabled;
        searchModel.AvatarEnabled = _customerSettings.AllowCustomersToUploadAvatars;
        searchModel.FirstNameEnabled = _customerSettings.FirstNameEnabled;
        searchModel.LastNameEnabled = _customerSettings.LastNameEnabled;
        searchModel.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        searchModel.CompanyEnabled = _customerSettings.CompanyEnabled;
        searchModel.PhoneEnabled = _customerSettings.PhoneEnabled;
        searchModel.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;

        //search registered customers by default
        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
            searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

        //prepare available customer roles
        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<AddCustomerToConditionListModel> PrepareAddCustomerToConditionListModelAsync(AddCustomerToConditionSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter customers
        _ = int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
        _ = int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

        //get customers
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
            email: searchModel.SearchEmail,
            username: searchModel.SearchUsername,
            firstName: searchModel.SearchFirstName,
            lastName: searchModel.SearchLastName,
            dayOfBirth: dayOfBirth,
            monthOfBirth: monthOfBirth,
            company: searchModel.SearchCompany,
            phone: searchModel.SearchPhone,
            zipPostalCode: searchModel.SearchZipPostalCode,
            ipAddress: searchModel.SearchIpAddress,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new AddCustomerToConditionListModel().PrepareToGridAsync(searchModel, customers, () =>
        {
            return customers.SelectAwait(async customer =>
            {
                //fill in model values from the entity
                var customerModel = customer.ToModel<CustomerModel>();

                //convert dates to the user time
                customerModel.Email = (await _customerService.IsRegisteredAsync(customer))
                    ? customer.Email
                    : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                customerModel.FullName = await _customerService.GetCustomerFullNameAsync(customer);
                customerModel.Company = customer.Company;
                customerModel.Phone = customer.Phone;
                customerModel.ZipPostalCode = customer.ZipPostalCode;

                //fill in additional values (not existing in the entity)
                customerModel.CustomerRoleNames = string.Join(", ", (await _customerService.GetCustomerRolesAsync(customer)).Select(role => role.Name));

                return customerModel;
            });
        });

        return model;
    }

    #endregion

    #region Product condition mappings

    public virtual Task PrepareProductConditionMappingSearchModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
        where TModel : IProductConditionSupportedModel
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (entity != null)
        {
            var searchModel = new ProductConditionSearchModel();
            searchModel.EntityId = entity.Id;
            searchModel.EntityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

            //prepare page parameters
            searchModel.SetGridPageSize();

            model.ProductConditionSearchModel = searchModel;
        }

        return Task.CompletedTask;
    }

    public virtual async Task<ProductConditionListModel> PrepareProductConditionMappingListModelAsync(ProductConditionSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get product condition mappings
        var productConditionMappings = (await _conditionMappingService.GetEntityProductConditionsAsync(searchModel.EntityId, searchModel.EntityName)).ToPagedList(searchModel);

        //prepare list model
        var model = await new ProductConditionListModel().PrepareToGridAsync(searchModel, productConditionMappings, () =>
        {
            return productConditionMappings.SelectAwait(async ccm =>
            {
                //fill in model values from the entity
                var ccmm = ccm.ToModel<ProductConditionModel>();

                var product = await _productService.GetProductByIdAsync(ccm.ProductId);
                ccmm.ProductName = product.Name;
                ccmm.Published = product.Published;

                return ccmm;
            });
        });

        return model;
    }

    public virtual async Task<AddProductToConditionSearchModel> PrepareAddProductToConditionSearchModelAsync<TEntity>(AddProductToConditionSearchModel searchModel, TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        searchModel.EntityId = entity.Id;
        searchModel.EntityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //a vendor should have access only to his products
        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
        searchModel.AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts;

        //prepare available categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

        //prepare available manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

        //prepare available product types
        await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

        //prepare available warehouses
        await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

        searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

        //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.All")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.PublishedOnly")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly")
        });

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<AddProductToConditionListModel> PrepareAddProductToConditionListModelAsync(AddProductToConditionSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            searchModel.SearchVendorId = currentVendor.Id;
        var categoryIds = new List<int> { searchModel.SearchCategoryId };
        if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
        {
            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
        }

        //get products
        var products = await _productService.SearchProductsAsync(showHidden: true,
            categoryIds: categoryIds,
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            warehouseId: searchModel.SearchWarehouseId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: overridePublished);

        //prepare list model
        var model = await new AddProductToConditionListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                //fill in model values from the entity
                var productModel = product.ToModel<ProductModel>();

                //little performance optimization: ensure that "FullDescription" is not returned
                productModel.FullDescription = string.Empty;

                //fill in additional values (not existing in the entity)
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                return productModel;
            });
        });

        return model;
    }

    #endregion

    #endregion
}
