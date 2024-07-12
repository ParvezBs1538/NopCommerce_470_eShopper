using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Helpers;
using NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache;
using NopStation.Plugin.Widgets.VendorShop.Models.ProductTabs;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public class ProductTabModelFactory : IProductTabModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IProductTabService _productTabService;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICurrencyService _currencyService;


        #endregion

        #region Ctor

        public ProductTabModelFactory(
            ICustomerService customerService,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService,
            IProductService productService,
            ILocalizationService localizationService,
            IProductModelFactory productModelFactory,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IProductTabService productTabService,
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            ICurrencyService currencyService)
        {
            _customerService = customerService;
            _staticCacheManager = staticCacheManager;
            _pictureService = pictureService;
            _productService = productService;
            _localizationService = localizationService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _productTabService = productTabService;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
        }

        #endregion

        #region Utlities 

        protected async Task<PictureModel> PreparePictureModelAsync(ProductTab productTab)
        {
            return new PictureModel()
            {
                ImageUrl = await _pictureService.GetPictureUrlAsync(productTab.PictureId),
                AlternateText = productTab.PictureAlt,
                Title = productTab.PictureTitle
            };
        }

        #endregion

        #region Methods

        public async Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(List<ProductTab> productTabs)
        {
            if (productTabs == null)
                throw new ArgumentNullException(nameof(productTabs));

            var model = new List<ProductTabModel>();
            foreach (var productTab in productTabs)
                model.Add(await PrepareProductTabModelAsync(productTab));
            return model;
        }

        public async Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(int vendorId, string widgetZone)
        {
            if (string.IsNullOrEmpty(widgetZone))
                throw new ArgumentNullException(nameof(widgetZone));

            if (!ProductTabHelper.TryGetWidgetZoneId(widgetZone, out var widgetZoneId))
                return new List<ProductTabModel>();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ProductTabCacheEventConsumer.GetProductTabModelKey(vendorId),
                                            vendorId,
                                            widgetZoneId,
                                            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                                            await _workContext.GetWorkingLanguageAsync(),
                                            await _storeContext.GetCurrentStoreAsync());

            var productTabModels = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var productTabs = (await _productTabService.GetAllProductTabsAsync(vendorId, new List<int> { widgetZoneId },
                    true, (await _storeContext.GetCurrentStoreAsync()).Id, true)).ToList();
                var productTabModelList = await PrepareProductTabListModelAsync(productTabs);

                return productTabModelList;
            });

            return productTabModels;
        }

        public async Task<ProductTabModel> PrepareProductTabModelAsync(ProductTab productTab)
        {
            if (productTab == null)
                throw new ArgumentNullException(nameof(productTab));

            var model = new ProductTabModel
            {
                Id = productTab.Id,
                AutoPlay = productTab.AutoPlay,
                RTL = (await _workContext.GetWorkingLanguageAsync()).Rtl,
                CustomCssClass = productTab.CustomCssClass,
                AutoPlayHoverPause = productTab.AutoPlayHoverPause,
                AutoPlayTimeout = productTab.AutoPlayTimeout,
                Center = productTab.Center,
                LazyLoad = productTab.LazyLoad,
                LazyLoadEager = productTab.LazyLoadEager,
                Loop = productTab.Loop,
                Margin = productTab.Margin,
                Nav = productTab.Nav,
                StartPosition = productTab.StartPosition,
                CustomUrl = productTab.CustomUrl
            };
            if (productTab.DisplayTitle)
            {
                model.DisplayTitle = productTab.DisplayTitle;
                model.Title = await _localizationService.GetLocalizedAsync(productTab, x => x.TabTitle);
            }
            model.Picture = await PreparePictureModelAsync(productTab);

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ProductTabCacheEventConsumer.GetProductTabItemModelKey(productTab.VendorId),
                productTab.VendorId,
                productTab,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _workContext.GetWorkingLanguageAsync(),
                await _storeContext.GetCurrentStoreAsync());

            model.Items = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var productTabItemModels = new List<ProductTabItemModel>();
                var productTabItems = _productTabService.GetProductTabItemsByProductTabId(productTab.Id);

                foreach (var item in productTabItems)
                    productTabItemModels.Add(await PrepareProductTabItemModelAsync(item, productTab.VendorId));
                return productTabItemModels;
            });

            return model;
        }

        private async Task<ProductTabItemModel> PrepareProductTabItemModelAsync(ProductTabItem item, int vendorId)
        {
            var model = new ProductTabItemModel()
            {
                Name = await _localizationService.GetLocalizedAsync(item, x => x.Name),
                Id = item.Id
            };

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ProductTabCacheEventConsumer.GetProductTabItemProductModelKey(vendorId),
                vendorId,
                item,
                await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                await _workContext.GetCurrentVendorAsync(),
                await _storeContext.GetCurrentStoreAsync());

            var productIds = _productTabService.GetProductTabItemProductsByProductTabItemId(item.Id).Select(x => x.ProductId).ToArray();
            var publishedProducts = (await _productService.GetProductsByIdsAsync(productIds)).Where(p => p.Published).ToList();

            publishedProducts = await publishedProducts.WhereAwait(async p =>
                                                        await _aclService.AuthorizeAsync(p)
                                                        && await _storeMappingService.AuthorizeAsync(p)
                                                        && _productService.ProductIsAvailable(p)).ToListAsync();

            model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(publishedProducts)).ToList();

            return model;
        }

        #endregion



    }
}
