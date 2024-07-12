using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;
using NopStation.Plugin.Widgets.SmartCarousels.Helpers;
using NopStation.Plugin.Widgets.SmartCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Factories;

public partial class SmartCarouselModelFactory : ISmartCarouselModelFactory
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ISmartCarouselService _carouselService;
    private readonly IProductService _productService;
    private readonly IPictureService _pictureService;
    private readonly ISettingService _settingService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IWidgetZoneModelFactory _widgetZoneModelFactory;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;

    #endregion

    #region Ctor

    public SmartCarouselModelFactory(IStoreContext storeContext,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        ILocalizedModelFactory localizedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        ISmartCarouselService carouselService,
        IProductService productService,
        IPictureService pictureService,
        ISettingService settingService,
        IDateTimeHelper dateTimeHelper,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IScheduleModelFactory scheduleModelFactory,
        IConditionModelFactory conditionModelFactory,
        IWidgetZoneModelFactory widgetZoneModelFactory,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IVendorService vendorService)
    {
        _storeContext = storeContext;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _localizedModelFactory = localizedModelFactory;
        _baseAdminModelFactory = baseAdminModelFactory;
        _localizationService = localizationService;
        _urlRecordService = urlRecordService;
        _carouselService = carouselService;
        _productService = productService;
        _pictureService = pictureService;
        _settingService = settingService;
        _dateTimeHelper = dateTimeHelper;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _scheduleModelFactory = scheduleModelFactory;
        _conditionModelFactory = conditionModelFactory;
        _widgetZoneModelFactory = widgetZoneModelFactory;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _vendorService = vendorService;
    }

    #endregion

    #region Utilities

    protected async Task PrepareWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //prepare available widget zones
        var availableWidgetZoneItems = SmartCarouselHelper.GetWidgetZoneSelectList();
        foreach (var widgetZoneItem in availableWidgetZoneItems)
        {
            items.Add(widgetZoneItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = ""
            });
    }

    protected async Task PrepareProductSourceTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableProductSourceTypeItems = await ProductSourceType.BestSellers.ToSelectListAsync(false);
        foreach (var typeItem in availableProductSourceTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareCarouselTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableCarouselTypeItems = await CarouselType.Product.ToSelectListAsync(false);
        foreach (var typeItem in availableCarouselTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PreparePaginationTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availablePaginationTypeItems = await PaginationType.Bullets.ToSelectListAsync(true);
        foreach (var typeItem in availablePaginationTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareBackgroundTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableBackgroundTypeItems = await BackgroundType.Picture.ToSelectListAsync(false);
        foreach (var typeItem in availableBackgroundTypeItems)
        {
            items.Add(typeItem);
        }

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
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.List.SearchActive.Active"),
            Value = "1"
        });
        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.List.SearchActive.Inactive"),
            Value = "2"
        });

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected virtual SmartCarouselProductSearchModel PrepareCarouselProductSearchModel(SmartCarouselProductSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        searchModel.CarouselId = carousel.Id;

        return searchModel;
    }

    protected virtual SmartCarouselManufacturerSearchModel PrepareCarouselManufacturerSearchModel(SmartCarouselManufacturerSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        searchModel.CarouselId = carousel.Id;

        return searchModel;
    }

    protected virtual SmartCarouselCategorySearchModel PrepareCarouselCategorySearchModel(SmartCarouselCategorySearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        searchModel.CarouselId = carousel.Id;

        return searchModel;
    }

    protected virtual SmartCarouselVendorSearchModel PrepareCarouselVendorSearchModel(SmartCarouselVendorSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        searchModel.CarouselId = carousel.Id;

        return searchModel;
    }

    protected virtual SmartCarouselPictureSearchModel PrepareCarouselPictureSearchModel(SmartCarouselPictureSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        searchModel.CarouselId = carousel.Id;

        return searchModel;
    }

    #endregion

    #region Methods

    #region Configuration

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var carouselSettings = await _settingService.LoadSettingAsync<SmartCarouselSettings>(storeId);

        var model = carouselSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return model;

        model.EnableCarousel_OverrideForStore = await _settingService.SettingExistsAsync(carouselSettings, x => x.EnableCarousel, storeId);
        model.EnableAjaxLoad_OverrideForStore = await _settingService.SettingExistsAsync(carouselSettings, x => x.EnableAjaxLoad, storeId);

        return model;
    }

    #endregion

    #region Carousels

    public async virtual Task<SmartCarouselSearchModel> PrepareCarouselSearchModelAsync(SmartCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        await PrepareWidgetZonesAsync(searchModel.AvailableWidgetZones, true);
        await PrepareProductSourceTypesAsync(searchModel.AvailableProductSources, true);
        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);
        await PrepareCarouselTypesAsync(searchModel.AvailableCarouselTypes, true);

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        return searchModel;
    }

    public async virtual Task<SmartCarouselListModel> PrepareCarouselListModelAsync(SmartCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        //get carousels
        var carousels = await _carouselService.GetAllCarouselsAsync(
            showHidden: true,
            overridePublished: overridePublished,
            productSourceTypeId: searchModel.SearchProductSourceId,
            keywords: searchModel.SearchKeyword,
            storeId: searchModel.SearchStoreId,
            carouselTypeId: searchModel.SearchCarouselTypeId,
            widgetZone: searchModel.SearchWidgetZone,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new SmartCarouselListModel().PrepareToGridAsync(searchModel, carousels, () =>
        {
            return carousels.SelectAwait(async carousel =>
            {
                return await PrepareCarouselModelAsync(null, carousel, true);
            });
        });

        return model;
    }

    public async Task<SmartCarouselModel> PrepareCarouselModelAsync(SmartCarouselModel model, SmartCarousel carousel, bool excludeProperties = false)
    {
        Func<CarouselLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (carousel != null)
        {
            if (model == null)
            {
                model = carousel.ToModel<SmartCarouselModel>();

                model.ProductSourceTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.ProductSourceType);
                model.BackgroundTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.BackgroundType);
                model.CarouselTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.CarouselType);

                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Title = await _localizationService.GetLocalizedAsync(carousel, entity => entity.Title, languageId, false, false);
                        locale.CustomUrl = await _localizationService.GetLocalizedAsync(carousel, entity => entity.CustomUrl, languageId, false, false);
                    };
                }
            }

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.UpdatedOnUtc, DateTimeKind.Utc);

            if (!excludeProperties)
            {
                PrepareCarouselProductSearchModel(model.CarouselProductSearchModel, carousel);
                PrepareCarouselManufacturerSearchModel(model.CarouselManufacturerSearchModel, carousel);
                PrepareCarouselCategorySearchModel(model.CarouselCategorySearchModel, carousel);
                PrepareCarouselVendorSearchModel(model.CarouselVendorSearchModel, carousel);
                PrepareCarouselPictureSearchModel(model.CarouselPictureSearchModel, carousel);

                //prepare customer condition mapping model
                await _conditionModelFactory.PrepareCustomerConditionMappingSearchModelAsync(model, carousel);

                //prepare product condition mapping model
                await _conditionModelFactory.PrepareProductConditionMappingSearchModelAsync(model, carousel);
            }
        }

        if (!excludeProperties)
        {
            await PrepareBackgroundTypesAsync(model.AvaliableBackgroundTypes, false);
            await PrepareProductSourceTypesAsync(model.AvailableProductSources, false);
            await PrepareCarouselTypesAsync(model.AvailableCarouselTypes, false);
            await PreparePaginationTypesAsync(model.AvailablePaginationTypes, false);

            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, carousel, excludeProperties);

            //prepare model schedule mappings
            await _scheduleModelFactory.PrepareScheduleMappingModelAsync(model, carousel);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, carousel, excludeProperties);

            //prepare model widget zone mappings
            await _widgetZoneModelFactory.PrepareWidgetZoneMappingSearchModelAsync(model, carousel);
            await _widgetZoneModelFactory.PrepareAddWidgetZoneMappingModelAsync(model, carousel, true, SmartCarouselHelper.GetCustomWidgetZones());
        }

        if (carousel == null)
        {
            model.Active = true;
            model.EnableAutoPlay = true;
            model.AutoPlayTimeout = 5000;
            model.AutoPlayHoverPause = true;
            model.MaxProductsToShow = 10;
            model.EnableLoop = true;
            model.EnableLazyLoad = true;
            model.EnableNavigation = true;
        }

        return model;
    }

    #endregion

    #region Carousel products

    public async Task<SmartCarouselProductListModel> PrepareCarouselProductListModelAsync(SmartCarouselProductSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        var carouselProducts = (await _carouselService.GetCarouselProductMappingsByCarouselIdAsync(searchModel.CarouselId)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new SmartCarouselProductListModel().PrepareToGridAsync(searchModel, carouselProducts, () =>
        {
            //fill in model values from the entity
            return carouselProducts.SelectAwait(async carouselProduct =>
            {
                var product = await _productService.GetProductByIdAsync(carouselProduct.ProductId);
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                var cpm = carouselProduct.ToModel<SmartCarouselProductModel>();
                cpm.ProductName = product.Name;
                cpm.PictureUrl = await _pictureService.GetPictureUrlAsync(picture.Id, 75);

                return cpm;
            });
        });

        return model;
    }

    public async Task<AddProductToCarouselSearchModel> PrepareAddProductToCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel)
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

    public async Task<AddProductToCarouselListModel> PrepareAddProductToCarouselListModelAsync(AddProductToCarouselSearchModel searchModel)
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
        var model = await new AddProductToCarouselListModel().PrepareToGridAsync(searchModel, products, () =>
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

    #region Carousel categories

    public async Task<SmartCarouselCategoryListModel> PrepareCarouselCategoryListModelAsync(SmartCarouselCategorySearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        var carouselCategories = (await _carouselService.GetCarouselCategoryMappingsByCarouselIdAsync(searchModel.CarouselId)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new SmartCarouselCategoryListModel().PrepareToGridAsync(searchModel, carouselCategories, () =>
        {
            //fill in model values from the entity
            return carouselCategories.SelectAwait(async carouselCategory =>
            {
                var category = await _categoryService.GetCategoryByIdAsync(carouselCategory.CategoryId);
                var ccm = carouselCategory.ToModel<SmartCarouselCategoryModel>();
                ccm.CategoryName = await _categoryService.GetFormattedBreadCrumbAsync(category);

                return ccm;
            });
        });

        return model;
    }

    public Task<AddCategoryToCarouselSearchModel> PrepareAddCategoryToCarouselSearchModelAsync(AddCategoryToCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<AddCategoryToCarouselListModel> PrepareAddCategoryToCarouselListModelAsync(AddCategoryToCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get categories
        var categories = await _categoryService.GetAllCategoriesAsync(showHidden: true,
            categoryName: searchModel.SearchCategoryName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddCategoryToCarouselListModel().PrepareToGridAsync(searchModel, categories, () =>
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

    #region Carousel manufacturers

    public async Task<SmartCarouselManufacturerListModel> PrepareCarouselManufacturerListModelAsync(SmartCarouselManufacturerSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        var carouselManufacturers = (await _carouselService.GetCarouselManufacturerMappingsByCarouselIdAsync(searchModel.CarouselId)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new SmartCarouselManufacturerListModel().PrepareToGridAsync(searchModel, carouselManufacturers, () =>
        {
            //fill in model values from the entity
            return carouselManufacturers.SelectAwait(async carouselManufacturer =>
            {
                var cmm = carouselManufacturer.ToModel<SmartCarouselManufacturerModel>();

                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(carouselManufacturer.ManufacturerId);
                cmm.ManufacturerId = manufacturer.Id;
                cmm.ManufacturerName = manufacturer.Name;

                return cmm;
            });
        });

        return model;
    }

    public Task<AddManufacturerToCarouselSearchModel> PrepareAddManufacturerToCarouselSearchModelAsync(AddManufacturerToCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<AddManufacturerToCarouselListModel> PrepareAddManufacturerToCarouselListModelAsync(AddManufacturerToCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get manufacturers
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true,
            manufacturerName: searchModel.SearchManufacturerName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddManufacturerToCarouselListModel().PrepareToGridAsync(searchModel, manufacturers, () =>
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

    #region Carousel vendors

    public async Task<SmartCarouselVendorListModel> PrepareCarouselVendorListModelAsync(SmartCarouselVendorSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        var carouselVendors = (await _carouselService.GetCarouselVendorMappingsByCarouselIdAsync(searchModel.CarouselId)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new SmartCarouselVendorListModel().PrepareToGridAsync(searchModel, carouselVendors, () =>
        {
            //fill in model values from the entity
            return carouselVendors.SelectAwait(async carouselVendor =>
            {
                var vendor = await _vendorService.GetVendorByIdAsync(carouselVendor.VendorId);

                var cvm = carouselVendor.ToModel<SmartCarouselVendorModel>();
                cvm.VendorName = vendor.Name;

                return cvm;
            });
        });

        return model;
    }

    public Task<AddVendorToCarouselSearchModel> PrepareAddVendorToCarouselSearchModelAsync(AddVendorToCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<AddVendorToCarouselListModel> PrepareAddVendorToCarouselListModelAsync(AddVendorToCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get vendors
        var vendors = await _vendorService.GetAllVendorsAsync(showHidden: true,
            name: searchModel.SearchName,
            email: searchModel.SearchEmail,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddVendorToCarouselListModel().PrepareToGridAsync(searchModel, vendors, () =>
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

    #region Carousel pictures

    public virtual async Task<SmartCarouselPictureListModel> PrepareCarouselPictureListModelAsync(SmartCarouselPictureSearchModel searchModel, SmartCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        //get product pictures
        var pictureMappings = (await _carouselService.GetCarouselPictureMappingsByCarouselIdAsync(carousel.Id)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new SmartCarouselPictureListModel().PrepareToGridAsync(searchModel, pictureMappings, () =>
        {
            return pictureMappings.SelectAwait(async pictureMapping =>
            {
                return await PrepareCarouselPictureModelAsync(null, pictureMapping, carousel, true);
            });
        });

        return model;
    }

    public async Task<SmartCarouselPictureModel> PrepareCarouselPictureModelAsync(SmartCarouselPictureModel model,
        SmartCarouselPictureMapping pictureMapping, SmartCarousel carousel, bool excludeProperties = false)
    {
        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        Func<SmartCarouselPictureLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (pictureMapping != null)
        {
            if (model == null)
            {
                //fill in model values from the entity
                model = pictureMapping.ToModel<SmartCarouselPictureModel>();

                //fill in additional values (not existing in the entity)
                var picture = await _pictureService.GetPictureByIdAsync(pictureMapping.PictureId);

                model.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url;

                model.OverrideAltAttribute = picture?.AltAttribute;
                model.OverrideTitleAttribute = picture?.TitleAttribute;
            }

            if (!excludeProperties)
            {
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Label = await _localizationService.GetLocalizedAsync(pictureMapping, entity => entity.Label, languageId, false, false);
                    locale.RedirectUrl = await _localizationService.GetLocalizedAsync(pictureMapping, entity => entity.RedirectUrl, languageId, false, false);
                };
            }
        }

        if (!excludeProperties)
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
        }

        if (carousel == null)
        {
            model.CarouselId = carousel.Id;
        }

        return model;
    }

    #endregion

    #endregion
}
