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
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Models.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Services;
using NopStation.Plugin.Widgets.VendorShop.Services.Cache;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public partial class OCarouselModelFactory : IOCarouselModelFactory
    {
        #region Fields
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IOrderReportService _orderReportService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IOCarouselService _carouselService;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _cacheKeyService;
        private readonly ICustomerService _customerService;
        private readonly IVendorCategoryService _vendorCategoryService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public OCarouselModelFactory(IPictureService pictureService,
            IProductService productService,
            ILocalizationService localizationService,
            IStaticCacheManager staticCacheManager,
            IProductModelFactory productModelFactory,
            IStoreContext storeContext,
            IOrderReportService orderReportService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IOCarouselService carouselService,
            IWorkContext workContext,
            IStaticCacheManager cacheKeyService,
            ICustomerService customerService,
            IVendorCategoryService vendorCategoryService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            MediaSettings mediaSettings)
        {
            _pictureService = pictureService;
            _productService = productService;
            _localizationService = localizationService;
            _cacheManager = staticCacheManager;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _orderReportService = orderReportService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _carouselService = carouselService;
            _workContext = workContext;
            _cacheKeyService = cacheKeyService;
            _customerService = customerService;
            _vendorCategoryService = vendorCategoryService;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Utlities 

        protected async Task<IList<OCarouselModel.OCarouselCategoryModel>> PrepareCategoryListModelAsync(OCarousel carousel)
        {
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.OCarouselCacheEventConsumer.GetOCarouselCategoriesModelKey(carousel.VendorId),
                carousel,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _workContext.GetWorkingLanguageAsync(),
                currentStore);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {

                var cacheKey = VendorCategoryFilterCacheDefault.GetAllVendorCategoryCacheKey(carousel.VendorId, currentStore.Id);

                var categories = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    return await _vendorCategoryService.GetAllCategoriesByVendorId(carousel.VendorId);
                });

                categories = await categories.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                    .Take(carousel.NumberOfItemsToShow).ToListAsync();

                var listModel = new List<OCarouselModel.OCarouselCategoryModel>();
                var vendor = await _vendorService.GetVendorByIdAsync(carousel.VendorId);
                foreach (var category in categories)
                {
                    var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
                    var cm = new OCarouselModel.OCarouselCategoryModel()
                    {
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        SeName = vendor == null ? string.Empty : (await _urlRecordService.GetSeNameAsync(vendor)) + $"?categoryIds={category.Id}#vendor-shop-catalog",
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
        protected async Task<string> GetCarouselBackgroundImageAsync(OCarousel carousel)
        {
            var vendorId = (await _workContext.GetCurrentVendorAsync())?.Id ?? 0;
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.OCarouselCacheEventConsumer.GetOCarouselBackgroundPictureModelKey(carousel.VendorId),
                vendorId,
                carousel, _storeContext.GetCurrentStoreAsync());
            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var pictureUrl = await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId);
                return pictureUrl;
            });
        }

        protected CarouselType GetCarouselType(DataSourceTypeEnum dataSource)
        {
            if (dataSource == DataSourceTypeEnum.Categories)
                return CarouselType.Category;
            return CarouselType.Product;
        }

        #endregion

        #region Methods

        public async Task<OCarouselListModel> PrepareCarouselListModelAsync(IList<OCarousel> carousels)
        {
            if (carousels == null)
                throw new ArgumentNullException(nameof(carousels));

            var model = new OCarouselListModel();
            foreach (var carousel in carousels)
                model.OCarousels.Add(new OCarouselListModel.OCarouselOverviewModel()
                {
                    Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title),
                    DisplayTitle = carousel.DisplayTitle,
                    CarouselType = GetCarouselType(carousel.DataSourceTypeEnum),
                    Id = carousel.Id,
                    ShowBackgroundPicture = carousel.ShowBackgroundPicture,
                    BackgroundPictureUrl = carousel.ShowBackgroundPicture ? await GetCarouselBackgroundImageAsync(carousel) : ""
                });

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
                model.BackgroundPictureUrl = await GetCarouselBackgroundImageAsync(carousel);
            }

            if (carousel.DisplayTitle)
            {
                model.DisplayTitle = true;
                model.Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title);
            }

            if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.Categories)
            {
                model.Categories = await PrepareCategoryListModelAsync(carousel);
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.NewProducts)
            {
                var products = await _carouselService.GetProductsMarkedAsNewAsync(carousel.VendorId,
                      storeId: _storeContext.GetCurrentStore().Id
                  );

                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            }
            else if (carousel.DataSourceTypeEnum == DataSourceTypeEnum.BestSellers)
            {
                var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey);
                var report = await _cacheManager.GetAsync(cacheKey, async () =>
                   (await _orderReportService.BestSellersReportAsync(
                       vendorId: carousel.VendorId,
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
                var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(Infrastructure.Cache.OCarouselCacheEventConsumer.GetOCarouselCustomProductsModelKey(carousel.VendorId), carousel.VendorId, carousel.Id);
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