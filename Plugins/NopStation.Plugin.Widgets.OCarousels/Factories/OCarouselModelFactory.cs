using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Models;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Factories
{
    public partial class OCarouselModelFactory : IOCarouselModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderReportService _orderReportService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IOCarouselService _carouselService;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _cacheKeyService;

        #endregion

        #region Ctor

        public OCarouselModelFactory(ICustomerService customerService,
            ICategoryService categoryService,
            IPictureService pictureService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            ILocalizationService localizationService,
            MediaSettings mediaSettings,
            IStaticCacheManager staticCacheManager,
            IProductModelFactory productModelFactory,
            IManufacturerService manufacturerService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IStoreContext storeContext,
            IOrderReportService orderReportService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IOCarouselService carouselService,
            IWorkContext workContext,
            IStaticCacheManager cacheKeyService)
        {
            _customerService = customerService;
            _categoryService = categoryService;
            _pictureService = pictureService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _cacheManager = staticCacheManager;
            _productModelFactory = productModelFactory;
            _manufacturerService = manufacturerService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _storeContext = storeContext;
            _orderReportService = orderReportService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _carouselService = carouselService;
            _workContext = workContext;
            _cacheKeyService = cacheKeyService;
        }

        #endregion

        #region Utlities 

        protected async Task<IList<OCarouselModel.OCarouselManufacturerModel>> PrepareManufacturerListModel(OCarousel carousel)
        {
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_MANUFACTURERS_MODEL_KEY,
                carousel,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _workContext.GetWorkingLanguageAsync(),
                await _storeContext.GetCurrentStoreAsync());

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var manufacturers = await (await _manufacturerService.GetAllManufacturersAsync()).ToListAsync();
                manufacturers = await manufacturers.WhereAwait(async m => await _storeMappingService.AuthorizeAsync(m)).Take(carousel.NumberOfItemsToShow).ToListAsync();

                var listModel = new List<OCarouselModel.OCarouselManufacturerModel>();

                foreach (var manufacturer in manufacturers)
                {
                    var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
                    var mm = new OCarouselModel.OCarouselManufacturerModel()
                    {
                        Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(manufacturer),
                        PictureModel = new PictureModel
                        {
                            ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSize)).Url,
                            FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                            Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                                            ? picture.TitleAttribute
                                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), manufacturer.Name),
                            AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                                            ? picture.AltAttribute
                                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), manufacturer.Name)
                        },
                    };

                    listModel.Add(mm);
                }

                return listModel;
            });
        }

        protected async Task<IList<OCarouselModel.OCarouselCategoryModel>> PrepareCategoryListModelAsync(OCarousel carousel)
        {
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_CATEGORIES_MODEL_KEY,
                carousel,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _workContext.GetWorkingLanguageAsync(),
                await _storeContext.GetCurrentStoreAsync());

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var categories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync();

                categories = await categories.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                    .Take(carousel.NumberOfItemsToShow).ToListAsync();

                var listModel = new List<OCarouselModel.OCarouselCategoryModel>();
                foreach (var category in categories)
                {
                    var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
                    var cm = new OCarouselModel.OCarouselCategoryModel()
                    {
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(category),
                        PictureModel = new PictureModel
                        {
                            ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSize)).Url,
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

        protected async Task<string> GetCarouselBackgroundImage(OCarousel carousel)
        {
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_BACKGROUND_PICTURE_MODEL_KEY,
                carousel, _storeContext.GetCurrentStoreAsync());
            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var pictureUrl = await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId);
                return pictureUrl;
            });
        }

        protected CarouselType GetCarouselType(DataSourceTypeEnum dataSource)
        {
            return dataSource switch
            {
                DataSourceTypeEnum.HomePageCategories => CarouselType.Category,
                DataSourceTypeEnum.Manufacturers => CarouselType.Manufacturer,
                _ => CarouselType.Product,
            };
        }

        #endregion

        #region Methods

        public async Task<OCarouselListModel> PrepareCarouselListModelAsync(IList<OCarousel> carousels)
        {
            if (carousels == null)
                throw new ArgumentNullException(nameof(carousels));

            var model = new OCarouselListModel();
            foreach (var carousel in carousels)
            {
                model.OCarousels.Add(new OCarouselListModel.OCarouselOverviewModel()
                {
                    Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title),
                    DisplayTitle = carousel.DisplayTitle,
                    CarouselType = GetCarouselType(carousel.DataSourceTypeEnum),
                    Id = carousel.Id,
                    ShowBackgroundPicture = carousel.ShowBackgroundPicture,
                    BackgroundPictureUrl = carousel.ShowBackgroundPicture ? await GetCarouselBackgroundImage(carousel) : ""
                });
            }

            return model;
        }

        public async Task<OCarouselModel> PrepareCarouselModelAsync(OCarousel carousel)
        {
            if (carousel == null)
                throw new ArgumentNullException(nameof(carousel));

            var model = new OCarouselModel
            {
                Id = carousel.Id,
                AutoPlay = carousel.AutoPlay,
                RTL = (await _workContext.GetWorkingLanguageAsync()).Rtl,
                CustomCssClass = carousel.CustomCssClass,
                AutoPlayHoverPause = carousel.AutoPlayHoverPause,
                AutoPlayTimeout = carousel.AutoPlayTimeout,
                Center = carousel.Center,
                LazyLoad = carousel.LazyLoad,
                LazyLoadEager = carousel.LazyLoadEager,
                Loop = carousel.Loop,
                Nav = carousel.Nav,
                StartPosition = carousel.StartPosition,
                CarouselType = GetCarouselType(carousel.DataSourceTypeEnum)
            };

            if (carousel.ShowBackgroundPicture)
            {
                model.ShowBackgroundPicture = carousel.ShowBackgroundPicture;
                model.BackgroundPictureUrl = await GetCarouselBackgroundImage(carousel);
            }

            if (carousel.DisplayTitle)
            {
                model.DisplayTitle = true;
                model.Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title);
            }
            if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.HomePageCategories)
            {
                model.Categories = await PrepareCategoryListModelAsync(carousel);
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.HomePageProducts)
            {
                var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
                //ACL and store mapping
                products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
                //availability dates
                products = await products.Where(p => _productService.ProductIsAvailable(p)).Take(carousel.NumberOfItemsToShow).ToListAsync();

                var enumerable = await _productModelFactory.PrepareProductOverviewModelsAsync(products);
                model.Products = enumerable.ToList();
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.Manufacturers)
            {
                model.Manufacturers = await PrepareManufacturerListModel(carousel);
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.NewProducts)
            {
                var products = await _productService.GetProductsMarkedAsNewAsync(
                        storeId: _storeContext.GetCurrentStore().Id
                    );

                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.RecentlyViewedProducts)
            {
                var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(carousel.NumberOfItemsToShow);
                var enumerable = await _productModelFactory.PrepareProductOverviewModelsAsync(products);
                model.Products = enumerable.ToList();
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.BestSellers)
            {
                var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey);
                var report = await _cacheManager.GetAsync(cacheKey, async () =>
                   (await _orderReportService.BestSellersReportAsync(
                       createdFromUtc: DateTime.UtcNow.AddDays(-30),
                       storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                       pageSize: carousel.NumberOfItemsToShow)
                   ).ToList());

                //load products
                var products = await _productService.GetProductsByIdsAsync(report.Select(x => x.ProductId).ToArray());
                //ACL and store mapping
                products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
                //availability dates
                products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.CustomProducts)
            {
                var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_CUSTOMRODUCTIDS_MODEL_KEY, carousel.Id);
                var productIds = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    return (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(carousel.Id))
                        .Select(ci => ci.ProductId)
                        .ToArray();
                });

                var sp = (await _productService.GetProductsByIdsAsync(productIds)).Where(p => p.Published).ToList();
                sp = await sp.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
                var products = sp.Where(p => _productService.ProductIsAvailable(p)).ToList();
                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            return model;
        }

        #endregion
    }
}