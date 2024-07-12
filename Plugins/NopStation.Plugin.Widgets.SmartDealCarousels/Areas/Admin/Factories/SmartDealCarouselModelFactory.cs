using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;
using NopStation.Plugin.Widgets.SmartDealCarousels.Helpers;
using NopStation.Plugin.Widgets.SmartDealCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Factories;

public partial class SmartDealCarouselModelFactory : ISmartDealCarouselModelFactory
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ISmartDealCarouselService _carouselService;
    private readonly IProductService _productService;
    private readonly IPictureService _pictureService;
    private readonly ISettingService _settingService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IWidgetZoneModelFactory _widgetZoneModelFactory;
    private readonly IDiscountService _discountService;

    #endregion

    #region Ctor

    public SmartDealCarouselModelFactory(IStoreContext storeContext,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        ILocalizedModelFactory localizedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizationService,
        IUrlRecordService urlRecordService,
        ISmartDealCarouselService carouselService,
        IProductService productService,
        IPictureService pictureService,
        ISettingService settingService,
        IDateTimeHelper dateTimeHelper,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IScheduleModelFactory scheduleModelFactory,
        IConditionModelFactory conditionModelFactory,
        IWidgetZoneModelFactory widgetZoneModelFactory,
        IDiscountService discountService)
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
        _discountService = discountService;
    }

    #endregion

    #region Utilities

    protected async Task PrepareWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //prepare available widget zones
        var availableWidgetZoneItems = SmartDealCarouselHelper.GetWidgetZoneSelectList();
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

        var availableProductSourceTypeItems = await ProductSourceType.Discount.ToSelectListAsync(false);
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

    protected async Task PreparePositionTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availablePositionTypeItems = await PositionType.Left.ToSelectListAsync(true);
        foreach (var typeItem in availablePositionTypeItems)
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

    public virtual async Task PrepareDiscountsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //prepare available discount types
        var availableDiscounts = (await _discountService.GetAllDiscountsAsync())
            .Where(d => d.DiscountType == DiscountType.AssignedToSkus ||
                d.DiscountType == DiscountType.AssignedToManufacturers ||
                d.DiscountType == DiscountType.AssignedToCategories)
            .ToList();

        foreach (var discount in availableDiscounts)
        {
            items.Add(new SelectListItem()
            {
                Value = discount.Id.ToString(),
                Text = discount.Name
            });
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
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive.Active"),
            Value = "1"
        });
        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive.Inactive"),
            Value = "2"
        });

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected virtual SmartDealCarouselProductSearchModel PrepareCarouselProductSearchModel(SmartDealCarouselProductSearchModel searchModel, SmartDealCarousel carousel)
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
        var carouselSettings = await _settingService.LoadSettingAsync<SmartDealCarouselSettings>(storeId);

        var model = carouselSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return model;

        model.EnableCarousel_OverrideForStore = await _settingService.SettingExistsAsync(carouselSettings, x => x.EnableCarousel, storeId);
        model.EnableAjaxLoad_OverrideForStore = await _settingService.SettingExistsAsync(carouselSettings, x => x.EnableAjaxLoad, storeId);
        model.CarouselPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(carouselSettings, x => x.CarouselPictureSize, storeId);

        return model;
    }

    #endregion

    #region Carousels

    public virtual async Task<SmartDealCarouselSearchModel> PrepareCarouselSearchModelAsync(SmartDealCarouselSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        await PrepareWidgetZonesAsync(searchModel.AvailableWidgetZones, true);
        await PrepareProductSourceTypesAsync(searchModel.AvailableProductSources, true);
        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        return searchModel;
    }

    public virtual async Task<SmartDealCarouselListModel> PrepareCarouselListModelAsync(SmartDealCarouselSearchModel searchModel)
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
            widgetZone: searchModel.SearchWidgetZone,
            pageSize: searchModel.PageSize,
            pageIndex: searchModel.Page - 1);

        //prepare list model
        var model = await new SmartDealCarouselListModel().PrepareToGridAsync(searchModel, carousels, () =>
        {
            return carousels.SelectAwait(async carousel =>
            {
                return await PrepareCarouselModelAsync(null, carousel, true);
            });
        });

        return model;
    }

    public async Task<SmartDealCarouselModel> PrepareCarouselModelAsync(SmartDealCarouselModel model, SmartDealCarousel carousel, bool excludeProperties = false)
    {
        Func<CarouselLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (carousel != null)
        {
            if (model == null)
            {
                model = carousel.ToModel<SmartDealCarouselModel>();

                model.ProductSourceTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.ProductSourceType);
                model.BackgroundTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.BackgroundType);
            }

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.UpdatedOnUtc, DateTimeKind.Utc);

            if (!excludeProperties)
            {
                PrepareCarouselProductSearchModel(model.CarouselProductSearchModel, carousel);

                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Title = await _localizationService.GetLocalizedAsync(carousel, entity => entity.Title, languageId, false, false);
                    locale.CustomUrl = await _localizationService.GetLocalizedAsync(carousel, entity => entity.CustomUrl, languageId, false, false);
                };

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
            await PreparePaginationTypesAsync(model.AvailablePaginationTypes, false);
            await PreparePositionTypesAsync(model.AvailablePositionTypes, false);
            await PrepareDiscountsAsync(model.AvaliableDiscounts, false);

            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, carousel, excludeProperties);

            //prepare model schedule mappings
            await _scheduleModelFactory.PrepareScheduleMappingModelAsync(model, carousel);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, carousel, excludeProperties);

            //prepare model widget zone mappings
            await _widgetZoneModelFactory.PrepareWidgetZoneMappingSearchModelAsync(model, carousel);
            await _widgetZoneModelFactory.PrepareAddWidgetZoneMappingModelAsync(model, carousel, true, SmartDealCarouselHelper.GetCustomWidgetZones());
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

    public async Task<SmartDealCarouselProductListModel> PrepareCarouselProductListModelAsync(SmartDealCarouselProductSearchModel searchModel, SmartDealCarousel carousel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        var carouselProducts = (await _carouselService.GetCarouselProductMappingsByCarouselIdAsync(searchModel.CarouselId)).ToPagedList(searchModel);

        //prepare grid model
        var model = await new SmartDealCarouselProductListModel().PrepareToGridAsync(searchModel, carouselProducts, () =>
        {
            //fill in model values from the entity
            return carouselProducts.SelectAwait(async carouselProduct =>
            {
                var product = await _productService.GetProductByIdAsync(carouselProduct.ProductId);
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                var cpm = carouselProduct.ToModel<SmartDealCarouselProductModel>();
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

    #endregion
}
