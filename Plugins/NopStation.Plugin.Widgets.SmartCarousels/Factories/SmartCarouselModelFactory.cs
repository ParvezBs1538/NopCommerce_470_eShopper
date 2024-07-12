using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Payments;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;
using NopStation.Plugin.Widgets.SmartCarousels.Infrastructure.Cache;
using NopStation.Plugin.Widgets.SmartCarousels.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartCarousels.Factories;

public partial class SmartCarouselModelFactory : ISmartCarouselModelFactory
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IPictureService _pictureService;
    private readonly IProductService _productService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ILocalizationService _localizationService;
    private readonly MediaSettings _mediaSettings;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
    private readonly IStoreContext _storeContext;
    private readonly IOrderReportService _orderReportService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IAclService _aclService;
    private readonly ISmartCarouselService _carouselService;
    private readonly IWorkContext _workContext;
    private readonly INopStationContext _nopStationContext;

    #endregion

    #region Ctor

    public SmartCarouselModelFactory(ICustomerService customerService,
        IPictureService pictureService,
        IProductService productService,
        IUrlRecordService urlRecordService,
        ILocalizationService localizationService,
        MediaSettings mediaSettings,
        IStaticCacheManager staticCacheManager,
        IProductModelFactory productModelFactory,
        IRecentlyViewedProductsService recentlyViewedProductsService,
        IStoreContext storeContext,
        IOrderReportService orderReportService,
        IStoreMappingService storeMappingService,
        IAclService aclService,
        ISmartCarouselService carouselService,
        IWorkContext workContext,
        INopStationContext nopStationContext)
    {
        _customerService = customerService;
        _pictureService = pictureService;
        _productService = productService;
        _urlRecordService = urlRecordService;
        _localizationService = localizationService;
        _mediaSettings = mediaSettings;
        _cacheManager = staticCacheManager;
        _productModelFactory = productModelFactory;
        _recentlyViewedProductsService = recentlyViewedProductsService;
        _storeContext = storeContext;
        _orderReportService = orderReportService;
        _storeMappingService = storeMappingService;
        _aclService = aclService;
        _carouselService = carouselService;
        _workContext = workContext;
        _nopStationContext = nopStationContext;
    }

    #endregion

    #region Utlities 

    protected async Task<IList<SmartCarousel>> GetCaruselsAsync(string widgetZone)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var productId = 0;

        if ((await _nopStationContext.GetRouteNameAsync()).Equals("Product", StringComparison.InvariantCultureIgnoreCase))
            productId = _nopStationContext.GetRouteValue(NopRoutingDefaults.RouteValue.ProductId, 0);

        var cacheKey = new CacheKey(string.Format(ModelCacheDefaults.CarouselByWidgetZoneKey, widgetZone, store.Id, productId), ModelCacheDefaults.CarouselByWidgetZoneKeyPrefix);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var carousels = await _carouselService.GetAllCarouselsAsync(
            productId: productId,
            storeId: store.Id,
            validScheduleOnly: true,
            overrideProduct: true,
            widgetZone: widgetZone);
            return carousels;
        });

    }

    protected async Task<IList<CarouselModel.CategoryModel>> PrepareCategoryListModelAsync(SmartCarousel carousel)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.CategoryListModelKey,
            carousel,
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _workContext.GetWorkingLanguageAsync(),
            await _storeContext.GetCurrentStoreAsync());

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var categories = await _carouselService.GetCategoriesByCarouselIdAsync(carousel.Id);

            categories = await categories.WhereAwait(async p =>
                await _aclService.AuthorizeAsync(p) &&
                await _storeMappingService.AuthorizeAsync(p))
                .ToListAsync();

            var listModel = new List<CarouselModel.CategoryModel>();
            foreach (var category in categories)
            {
                var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
                var cm = new CarouselModel.CategoryModel()
                {
                    Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(category),
                    PictureModel = new PictureModel
                    {
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.CategoryThumbPictureSize)).Url,
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                                        ? picture.TitleAttribute
                                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), category.Name),
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                                        ? picture.AltAttribute
                                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), category.Name)
                    },
                };

                listModel.Add(cm);
            }

            return listModel;
        });
    }

    protected async Task<IList<CarouselModel.ManufacturerModel>> PrepareManufacturerListModelAsync(SmartCarousel carousel)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.ManufacturerListModelKey,
            carousel,
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _workContext.GetWorkingLanguageAsync(),
            await _storeContext.GetCurrentStoreAsync());

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var manufacturers = await _carouselService.GetManufacturersByCarouselIdAsync(carousel.Id);

            manufacturers = await manufacturers.WhereAwait(async p =>
                await _aclService.AuthorizeAsync(p) &&
                await _storeMappingService.AuthorizeAsync(p))
                .ToListAsync();

            var listModel = new List<CarouselModel.ManufacturerModel>();
            foreach (var manufacturer in manufacturers)
            {
                var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
                var cm = new CarouselModel.ManufacturerModel()
                {
                    Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(manufacturer),
                    PictureModel = new PictureModel
                    {
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ManufacturerThumbPictureSize)).Url,
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                                        ? picture.TitleAttribute
                                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), manufacturer.Name),
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                                        ? picture.AltAttribute
                                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), manufacturer.Name)
                    },
                };

                listModel.Add(cm);
            }

            return listModel;
        });
    }

    protected async Task<IList<CarouselModel.VendorModel>> PrepareVendorListModelAsync(SmartCarousel carousel)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.VendorListModelKey,
            carousel,
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _workContext.GetWorkingLanguageAsync(),
            await _storeContext.GetCurrentStoreAsync());

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var vendors = await _carouselService.GetVendorsByCarouselIdAsync(carousel.Id);

            var listModel = new List<CarouselModel.VendorModel>();
            foreach (var vendor in vendors)
            {
                var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
                var cm = new CarouselModel.VendorModel()
                {
                    Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(vendor),
                    PictureModel = new PictureModel
                    {
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.VendorThumbPictureSize)).Url,
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                                        ? picture.TitleAttribute
                                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), vendor.Name),
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                                        ? picture.AltAttribute
                                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), vendor.Name)
                    },
                };

                listModel.Add(cm);
            }

            return listModel;
        });
    }

    protected async Task<IList<CarouselModel.CarouselPictureModel>> PreparePictureListModelAsync(SmartCarousel carousel)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.PictureListModelKey,
            carousel,
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _workContext.GetWorkingLanguageAsync(),
            await _storeContext.GetCurrentStoreAsync());

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var mappings = await _carouselService.GetCarouselPictureMappingsByCarouselIdAsync(carousel.Id);

            var listModel = new List<CarouselModel.CarouselPictureModel>();
            foreach (var mapping in mappings)
            {
                var picture = await _pictureService.GetPictureByIdAsync(mapping.PictureId);
                var cm = new CarouselModel.CarouselPictureModel()
                {
                    Label = await _localizationService.GetLocalizedAsync(mapping, x => x.Label),
                    RedirectUrl = await _localizationService.GetLocalizedAsync(mapping, x => x.RedirectUrl),
                    PictureModel = new PictureModel
                    {
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSize)).Url,
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        Title = picture?.TitleAttribute,
                        AlternateText = picture?.AltAttribute
                    },
                };

                listModel.Add(cm);
            }

            return listModel;
        });
    }

    protected async Task<bool> HasSliderItemsAsync(SmartCarousel carousel)
    {
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        switch (carousel.CarouselType)
        {
            case CarouselType.Product:
                if (carousel.ProductSourceType != ProductSourceType.CustomProducts)
                    return true;

                return (await _carouselService.GetProductsByCarouselIdAsync(carousel.Id, storeId, 1)).Any();
            case CarouselType.Manufacturer:
                return (await _carouselService.GetManufacturersByCarouselIdAsync(carousel.Id, storeId, 1)).Any();
            case CarouselType.Category:
                return (await _carouselService.GetCategoriesByCarouselIdAsync(carousel.Id, storeId, 1)).Any();
            case CarouselType.Vendor:
                return (await _carouselService.GetVendorsByCarouselIdAsync(carousel.Id, 1)).Any();
            case CarouselType.Picture:
            default:
                return (await _carouselService.GetCarouselPictureMappingsByCarouselIdAsync(carousel.Id)).Any();
        }
    }

    #endregion

    #region Methods

    public async Task<IList<CarouselOverviewModel>> PrepareCarouselAjaxListModelAsync(string widgetZone)
    {
        var roles = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
        var language = await _workContext.GetWorkingLanguageAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var carousels = await GetCaruselsAsync(widgetZone);

        var model = new List<CarouselOverviewModel>();
        foreach (var carousel in carousels)
        {
            if (!await HasSliderItemsAsync(carousel))
                continue;

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.CarouselOverviewModelKey,
                carousel, roles, language, store);

            var overviewModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var m = new CarouselOverviewModel()
                {
                    Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title),
                    DisplayTitle = carousel.DisplayTitle,
                    CarouselType = carousel.CarouselType,
                    Id = carousel.Id,
                    ShowBackground = carousel.ShowBackground,
                    BackgroundType = carousel.BackgroundType,
                    CustomUrl = await _localizationService.GetLocalizedAsync(carousel, x => x.CustomUrl),
                    CustomCssClass = carousel.CustomCssClass
                };

                if (carousel.ShowBackground)
                {
                    m.ShowBackground = carousel.ShowBackground;
                    m.BackgroundPictureUrl = await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId);
                    m.BackgroundType = carousel.BackgroundType;
                    m.BackgroundColor = carousel.BackgroundColor;
                }

                return m;
            });

            model.Add(overviewModel);
        }

        return model;
    }

    public async Task<IList<CarouselModel>> PrepareCarouselListModelAsync(string widgetZone)
    {
        var carousels = await GetCaruselsAsync(widgetZone);

        return await carousels.WhereAwait(async c => await HasSliderItemsAsync(c))
            .SelectAwait(async c => await PrepareCarouselModelAsync(c)).ToListAsync();
    }

    public async Task<CarouselModel> PrepareCarouselModelAsync(SmartCarousel carousel)
    {
        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.CarouselModelKey,
            carousel,
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _workContext.GetWorkingLanguageAsync(),
            await _storeContext.GetCurrentStoreAsync());

        var model = await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var m = new CarouselModel
            {
                Id = carousel.Id,
                AutoPlay = carousel.EnableAutoPlay,
                CustomCssClass = carousel.CustomCssClass,
                AutoPlayHoverPause = carousel.AutoPlayHoverPause,
                AutoPlayTimeout = carousel.AutoPlayTimeout,
                Center = carousel.Center,
                LazyLoad = carousel.EnableLazyLoad,
                Loop = carousel.EnableLoop,
                Navigation = carousel.EnableNavigation,
                StartPosition = carousel.StartPosition,
                CarouselType = carousel.CarouselType,
                ShowBackground = carousel.ShowBackground,
                CustomUrl = await _localizationService.GetLocalizedAsync(carousel, x => x.CustomUrl),
                KeyboardControl = carousel.EnableKeyboardControl,
                KeyboardControlOnlyInViewport = carousel.KeyboardControlOnlyInViewport,
                Pagination = carousel.EnablePagination,
                PaginationClickable = carousel.PaginationClickable,
                PaginationDynamicBullets = carousel.PaginationDynamicBullets,
                PaginationDynamicMainBullets = carousel.PaginationDynamicMainBullets,
                PaginationType = carousel.PaginationType
            };

            if (carousel.ShowBackground)
            {
                m.ShowBackground = carousel.ShowBackground;
                m.BackgroundPictureUrl = await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId);
                m.BackgroundType = carousel.BackgroundType;
                m.BackgroundColor = carousel.BackgroundColor;
            }

            if (carousel.DisplayTitle)
            {
                m.DisplayTitle = true;
                m.Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title);
            }

            return m;
        });

        if (carousel.CarouselType == CarouselType.Category)
        {
            model.Categories = await PrepareCategoryListModelAsync(carousel);
        }
        else if (carousel.CarouselType == CarouselType.Manufacturer)
        {
            model.Manufacturers = await PrepareManufacturerListModelAsync(carousel);
        }
        else if (carousel.CarouselType == CarouselType.Vendor)
        {
            model.Vendors = await PrepareVendorListModelAsync(carousel);
        }
        else if (carousel.CarouselType == CarouselType.Picture)
        {
            model.Pictures = await PreparePictureListModelAsync(carousel);
        }
        else
        {
            if (carousel.ProductSourceType == ProductSourceType.HomePageProducts)
            {
                var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
                //ACL and store mapping
                products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
                //availability dates
                products = await products.Where(p => _productService.ProductIsAvailable(p)).Take(carousel.MaxProductsToShow).ToListAsync();

                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.ProductSourceType == ProductSourceType.NewProducts)
            {
                var products = (await _productService.GetProductsMarkedAsNewAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id))
                        .Take(carousel.MaxProductsToShow).ToList();

                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.ProductSourceType == ProductSourceType.RecentlyViewedProducts)
            {
                var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(carousel.MaxProductsToShow);
                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.ProductSourceType == ProductSourceType.BestSellers)
            {
                var cacheKey1 = _cacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey);
                var report = await _cacheManager.GetAsync(cacheKey1, async () =>
                   (await _orderReportService.BestSellersReportAsync(
                       createdFromUtc: DateTime.UtcNow.AddDays(-30),
                       storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                       ps: PaymentStatus.Paid,
                       pageSize: carousel.MaxProductsToShow)
                   ).ToList());

                //load products
                var products = await _productService.GetProductsByIdsAsync(report.Select(x => x.ProductId).ToArray());
                //ACL and store mapping
                products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
                //availability dates
                products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.ProductSourceType == ProductSourceType.CustomProducts)
            {
                var products = await _carouselService.GetProductsByCarouselIdAsync(carousel.Id, (await _storeContext.GetCurrentStoreAsync()).Id);
                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }

        }
        return model;
    }

    #endregion
}