using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductBadge.Domains;
using NopStation.Plugin.Widgets.ProductBadge.Services;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Factories;

public class BadgeModelFactory : IBadgeModelFactory
{
    #region Fields

    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IBadgeService _badgeService;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ICategoryService _categoryService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IStoreContext _storeContext;
    private readonly ICurrencyService _currencyService;
    private readonly ISettingService _settingService;
    private readonly CurrencySettings _currencySetting;

    #endregion

    #region Ctor

    public BadgeModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
       ILocalizationService localizationService,
       IBadgeService badgeService,
       ILocalizedModelFactory localizedModelFactory,
       IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
       ICategoryService categoryService,
       IUrlRecordService urlRecordService,
       IManufacturerService manufacturerService,
       IProductService productService,
       IVendorService vendorService,
       IAclSupportedModelFactory aclSupportedModelFactory,
       IStoreContext storeContext,
       ICurrencyService currencyService,
       ISettingService settingService,
       CurrencySettings currencySetting)
    {
        _baseAdminModelFactory = baseAdminModelFactory;
        _localizationService = localizationService;
        _badgeService = badgeService;
        _localizedModelFactory = localizedModelFactory;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _categoryService = categoryService;
        _urlRecordService = urlRecordService;
        _manufacturerService = manufacturerService;
        _productService = productService;
        _vendorService = vendorService;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _storeContext = storeContext;
        _currencyService = currencyService;
        _settingService = settingService;
        _currencySetting = currencySetting;
    }

    #endregion

    #region Utilities

    protected async Task PrepareSizesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableSizes = (await Size.Small.ToSelectListAsync(false)).ToList();
        foreach (var source in availableSizes)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PreparePositionTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availablePositionTypes = (await PositionType.Left.ToSelectListAsync(false)).ToList();
        foreach (var source in availablePositionTypes)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareBadgeTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableBadgeTypes = (await BadgeType.CustomProducts.ToSelectListAsync(true)).ToList();
        foreach (var source in availableBadgeTypes)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareDiscountBadgeTextFormatsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableDiscountBadgeTextFormats = (await DiscountBadgeTextFormat.TextAfterDiscount.ToSelectListAsync(true)).ToList();
        foreach (var source in availableDiscountBadgeTextFormats)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareContentTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableContentTypes = (await ContentType.Picture.ToSelectListAsync(false)).ToList();
        foreach (var source in availableContentTypes)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareShapeTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableShapeTypes = (await ShapeType.Circle.ToSelectListAsync(false)).ToList();
        foreach (var source in availableShapeTypes)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareCatalogTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableCatalogTypes = (await CatalogType.Products.ToSelectListAsync(false)).ToList();
        foreach (var source in availableCatalogTypes)
            items.Add(source);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.List.SearchActive.Active"),
            Value = "1"
        });
        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Badges.List.SearchActive.Inactive"),
            Value = "2"
        });

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected Task<BadgeCategorySearchModel> PrepareBadgeCategorySearchModelAsync(BadgeCategorySearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.BadgeId = badge?.Id ?? 0;

        return Task.FromResult(searchModel);
    }

    protected Task<BadgeProductSearchModel> PrepareBadgeProductSearchModelAsync(BadgeProductSearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.BadgeId = badge?.Id ?? 0;

        return Task.FromResult(searchModel);
    }

    protected Task<BadgeManufacturerSearchModel> PrepareBadgeManufacturerSearchModelAsync(BadgeManufacturerSearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.BadgeId = badge?.Id ?? 0;

        return Task.FromResult(searchModel);
    }

    protected Task<BadgeVendorSearchModel> PrepareBadgeVendorSearchModelAsync(BadgeVendorSearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.BadgeId = badge?.Id ?? 0;

        return Task.FromResult(searchModel);
    }

    #endregion

    #region Methods

    #region Badges

    public async Task<BadgeSearchModel> PrepareBadgeSearchModelAsync(BadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);
        await PrepareBadgeTypesAsync(searchModel.AvailableBadgeTypes, true);
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        return searchModel;
    }

    public async Task<BadgeListModel> PrepareBadgeListModelAsync(BadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        var badges = await _badgeService.GetAllBadgesAsync(
            showHidden: true,
            keywords: searchModel.SearchKeyword,
            storeId: searchModel.SearchStoreId,
            overridePublished: overridePublished,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        //prepare list model

        var model = await new BadgeListModel().PrepareToGridAsync(searchModel, badges, () =>
        {
            return badges.SelectAwait(async badge =>
            {
                return await PrepareBadgeModelAsync(null, badge, true);
            });
        });

        return model;
    }

    public async Task<BadgeModel> PrepareBadgeModelAsync(BadgeModel model, Badge badge, bool excludeProperties = false)
    {
        Func<BadgeLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (badge != null)
        {
            if (model == null)
            {
                model = badge.ToModel<BadgeModel>();

                model.BestSellOrderStatusIds = badge.BestSellOrderStatusIds.ToIntList();
                model.BestSellPaymentStatusIds = badge.BestSellPaymentStatusIds.ToIntList();
                model.BestSellShippingStatusIds = badge.BestSellShippingStatusIds.ToIntList();
                model.PositionTypeStr = await _localizationService.GetLocalizedEnumAsync(badge.PositionType);
                model.ContentTypeStr = await _localizationService.GetLocalizedEnumAsync(badge.ContentType);
            }

            if (!excludeProperties)
            {
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Text = await _localizationService.GetLocalizedAsync(badge, entity => entity.Text, languageId, false, false);
                };
            }
        }

        if (!excludeProperties)
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySetting.PrimaryStoreCurrencyId);
            model.CurrencyCode = currency?.CurrencyCode;

            await PrepareBadgeCategorySearchModelAsync(model.BadgeCategorySearchModel, badge);
            await PrepareBadgeManufacturerSearchModelAsync(model.BadgeManufactureSearchModel, badge);
            await PrepareBadgeProductSearchModelAsync(model.BadgeProductSearchModel, badge);
            await PrepareBadgeVendorSearchModelAsync(model.BadgeVendorSearchModel, badge);

            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, badge, excludeProperties);
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, badge, excludeProperties);

            await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, false);
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(model.AvailablePaymentStatuses, false);
            await _baseAdminModelFactory.PrepareShippingStatusesAsync(model.AvailableShippingStatuses, false);

            await PrepareSizesAsync(model.AvailableSizes, false);
            await PreparePositionTypesAsync(model.AvailablePositionTypes, false);
            await PrepareCatalogTypesAsync(model.AvailableCatalogTypes, false);
            await PrepareShapeTypesAsync(model.AvailableShapeTypes, false);
            await PrepareContentTypesAsync(model.AvailableContentTypes, false);
            await PrepareBadgeTypesAsync(model.AvailableBadgeTypes, false);
            await PrepareDiscountBadgeTextFormatsAsync(model.AvailableDiscountBadgeTextFormats, false);
        }

        if (badge == null)
        {
            model.Active = true;
            model.SizeId = (int)Size.Medium;
            model.BestSellSoldInDays = 30;
            model.BadgeTypeId = (int)BadgeType.CustomProducts;
            model.PositionTypeId = (int)PositionType.Right;
            model.ShapeTypeId = (int)ShapeType.Circle;

            model.BestSellOrderStatusIds = new List<int>
            {
                (int)OrderStatus.Complete
            };
            model.BestSellPaymentStatusIds = new List<int>
            {
                (int)PaymentStatus.Paid
            };
            model.BestSellShippingStatusIds = new List<int>
            {
                (int)ShippingStatus.Delivered
            };
        }

        return model;
    }

    #endregion

    #region Badge category mappings

    public async Task<BadgeCategoryListModel> PrepareBadgeCategoryListModelAsync(BadgeCategorySearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (badge == null)
            throw new ArgumentNullException(nameof(badge));

        var badgeCategories = (await _badgeService.GetBadgeCategoryMappingsAsync(searchModel.BadgeId, false)).ToPagedList(searchModel);

        var model = await new BadgeCategoryListModel().PrepareToGridAsync(searchModel, badgeCategories, () =>
        {
            return badgeCategories.SelectAwait(async bc =>
            {
                var cm = bc.ToModel<BadgeCategoryModel>();

                var category = await _categoryService.GetCategoryByIdAsync(bc.CategoryId);
                cm.CategoryName = await _categoryService.GetFormattedBreadCrumbAsync(category);
                cm.Published = category.Published;

                return cm;
            });
        });

        return model;
    }

    public virtual Task<AddCategoryToBadgeSearchModel> PrepareAddCategoryToBadgeSearchModelAsync(AddCategoryToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<AddCategoryToBadgeListModel> PrepareAddCategoryToBadgeListModelAsync(AddCategoryToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get categories
        var categories = await _categoryService.GetAllCategoriesAsync(showHidden: true,
            categoryName: searchModel.SearchCategoryName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddCategoryToBadgeListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                //fill in model values from the entity
                var categoryModel = category.ToModel<CategoryModel>();

                //fill in additional values (not existing in the entity)
                categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumbAsync(category);
                categoryModel.SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false);

                return categoryModel;
            });
        });

        return model;
    }

    #endregion

    #region Badge manufacturer mappings

    public async Task<BadgeManufacturerListModel> PrepareBadgeManufacturerListModelAsync(BadgeManufacturerSearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (badge == null)
            throw new ArgumentNullException(nameof(badge));

        //get manufacturers with applied badge
        var badgeManufacturers = (await _badgeService.GetBadgeManufacturerMappingsAsync(badge.Id, false)).ToPagedList(searchModel);

        var model = await new BadgeManufacturerListModel().PrepareToGridAsync(searchModel, badgeManufacturers, () =>
        {
            return badgeManufacturers.SelectAwait(async bm =>
            {
                var mm = bm.ToModel<BadgeManufacturerModel>();

                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(bm.ManufacturerId);
                mm.ManufacturerId = manufacturer.Id;
                mm.ManufacturerName = manufacturer.Name;
                mm.Published = manufacturer.Published;

                return mm;
            });
        });

        return model;
    }

    public Task<AddManufacturerToBadgeSearchModel> PrepareAddManufacturerToBadgeSearchModelAsync(AddManufacturerToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<AddManufacturerToBadgeListModel> PrepareAddManufacturerToBadgeListModelAsync(AddManufacturerToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get manufacturers
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true,
            manufacturerName: searchModel.SearchManufacturerName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddManufacturerToBadgeListModel().PrepareToGridAsync(searchModel, manufacturers, () =>
        {
            return manufacturers.SelectAwait(async manufacturer =>
            {
                var manufacturerModel = manufacturer.ToModel<ManufacturerModel>();
                manufacturerModel.SeName = await _urlRecordService.GetSeNameAsync(manufacturer, 0, true, false);

                return manufacturerModel;
            });
        });

        return model;
    }

    #endregion

    #region Badge product mappings

    public async Task<BadgeProductListModel> PrepareBadgeProductListModelAsync(BadgeProductSearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (badge == null)
            throw new ArgumentNullException(nameof(badge));

        //get products with applied badge
        var badgeProducts = (await _badgeService.GetBadgeProductMappingsAsync(badge.Id)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new BadgeProductListModel().PrepareToGridAsync(searchModel, badgeProducts, () =>
        {
            //fill in model values from the entity
            return badgeProducts.SelectAwait(async bp =>
            {
                var pm = bp.ToModel<BadgeProductModel>();

                var product = await _productService.GetProductByIdAsync(bp.ProductId);
                pm.ProductId = product.Id;
                pm.ProductName = product.Name;
                pm.Published = product.Published;

                return pm;
            });
        });

        return model;
    }

    public async Task<AddProductToBadgeSearchModel> PrepareAddProductToBadgeSearchModelAsync(AddProductToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

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

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return searchModel;
    }

    public async Task<AddProductToBadgeListModel> PrepareAddProductToBadgeListModelAsync(AddProductToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get products
        var products = await _productService.SearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddProductToBadgeListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                var productModel = product.ToModel<ProductModel>();
                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                return productModel;
            });
        });

        return model;
    }

    #endregion

    #region Badge vendor mappings

    public async Task<BadgeVendorListModel> PrepareBadgeVendorListModelAsync(BadgeVendorSearchModel searchModel, Badge badge)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (badge == null)
            throw new ArgumentNullException(nameof(badge));

        //get vendors with applied badge
        var badgeVendors = (await _badgeService.GetBadgeVendorMappingsAsync(badge.Id, false)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new BadgeVendorListModel().PrepareToGridAsync(searchModel, badgeVendors, () =>
        {
            //fill in model values from the entity
            return badgeVendors.SelectAwait(async bv =>
            {
                var vm = bv.ToModel<BadgeVendorModel>();

                var vendor = await _vendorService.GetVendorByIdAsync(bv.VendorId);
                vm.VendorId = vendor.Id;
                vm.VendorName = vendor.Name;
                vm.Active = vendor.Active;

                return vm;
            });
        });

        return model;
    }

    public Task<AddVendorToBadgeSearchModel> PrepareAddVendorToBadgeSearchModelAsync(AddVendorToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<AddVendorToBadgeListModel> PrepareAddVendorToBadgeListModelAsync(AddVendorToBadgeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get vendors
        var vendors = await _vendorService.GetAllVendorsAsync(showHidden: true,
            name: searchModel.SearchName,
            email: searchModel.SearchEmail,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddVendorToBadgeListModel().PrepareToGridAsync(searchModel, vendors, () =>
        {
            return vendors.SelectAwait(async vendor =>
            {
                var vendorModel = vendor.ToModel<VendorModel>();
                vendorModel.SeName = await _urlRecordService.GetSeNameAsync(vendor, 0, true, false);

                return vendorModel;
            });
        });

        return model;
    }

    #endregion

    #region Configuration

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var badgeSettings = await _settingService.LoadSettingAsync<ProductBadgeSettings>(storeId);

        var model = badgeSettings.ToSettingsModel<ConfigurationModel>();

        if (storeId <= 0)
            return model;

        model.EnableAjaxLoad_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.EnableAjaxLoad, storeId);
        model.ProductBoxWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.ProductBoxWidgetZone, storeId);
        model.ProductDetailsWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.ProductDetailsWidgetZone, storeId);
        model.SmallBadgeWidth_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.SmallBadgeWidth, storeId);
        model.MediumBadgeWidth_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.MediumBadgeWidth, storeId);
        model.LargeBadgeWidth_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.LargeBadgeWidth, storeId);
        model.IncreaseWidthInDetailsPageByPercentage_OverrideForStore = await _settingService.SettingExistsAsync(badgeSettings, x => x.IncreaseWidthInDetailsPageByPercentage, storeId);

        return model;
    }

    #endregion

    #endregion
}