using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Factories;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;
using NopStation.Plugin.Misc.WebApi.Models.Catalog;
using NopStation.Plugin.Misc.WebApi.Models.Common;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/home")]
    public class HomeApiController : BaseApiController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IOrderReportService _orderReportService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly WebApiSettings _webApiSettings;
        private readonly ICatalogApiModelFactory _catalogApiModelFactory;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICommonApiModelFactory _commonApiModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly VendorSettings _vendorSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IReturnRequestService _returnRequestService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IPermissionService _permissionService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public HomeApiController(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IOrderReportService orderReportService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            WebApiSettings webApiSettings,
            ICatalogApiModelFactory catalogApiModelFactory,
            ICommonModelFactory commonModelFactory,
            ICommonApiModelFactory commonApiModelFactory,
            IPictureService pictureService,
            ILanguageService languageService,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            VendorSettings vendorSettings,
            OrderSettings orderSettings,
            CustomerSettings customerSettings,
            IReturnRequestService returnRequestService,
            CatalogSettings catalogSettings,
            IPermissionService permissionService,
            StoreInformationSettings storeInformationSettings)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _orderReportService = orderReportService;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _webApiSettings = webApiSettings;
            _catalogApiModelFactory = catalogApiModelFactory;
            _commonModelFactory = commonModelFactory;
            _commonApiModelFactory = commonApiModelFactory;
            _pictureService = pictureService;
            _languageService = languageService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _vendorSettings = vendorSettings;
            _orderSettings = orderSettings;
            _customerSettings = customerSettings;
            _returnRequestService = returnRequestService;
            _catalogSettings = catalogSettings;
            _permissionService = permissionService;
            _storeInformationSettings = storeInformationSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<Language> EnsureLanguageIsActive(Language language)
        {
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language;
        }

        #endregion

        #region Methods

        [HttpGet("applandingsetting")]
        public virtual async Task<IActionResult> AppLandingSetting(bool appStart)
        {
            var response = new GenericResponseModel<AppConfigurationModel>();

            var model = new AppConfigurationModel
            {
                PrimaryThemeColor = _webApiSettings.PrimaryThemeColor,
                BottomBarActiveColor = _webApiSettings.BottomBarActiveColor,
                BottomBarInactiveColor = _webApiSettings.BottomBarInactiveColor,
                BottomBarBackgroundColor = _webApiSettings.BottomBarBackgroundColor,
                TopBarTextColor = _webApiSettings.TopBarTextColor,
                TopBarBackgroundColor = _webApiSettings.TopBarBackgroundColor,
                GradientStartingColor = _webApiSettings.GradientStartingColor,
                GradientMiddleColor = _webApiSettings.GradientMiddleColor,
                GradientEndingColor = _webApiSettings.GradientEndingColor,
                GradientEnabled = _webApiSettings.GradientEnabled,
                IOSProductPriceTextSize = _webApiSettings.IOSProductPriceTextSize,
                AndroidProductPriceTextSize = _webApiSettings.AndroidProductPriceTextSize,
                IonicProductPriceTextSize = _webApiSettings.IonicProductPriceTextSize,
                ShowHomepageSlider = _webApiSettings.ShowHomepageSlider,
                SliderAutoPlay = _webApiSettings.SliderAutoPlay,
                SliderAutoPlayTimeout = _webApiSettings.SliderAutoPlayTimeout,
                ShowFeaturedProducts = _webApiSettings.ShowFeaturedProducts,
                ShowBestsellersOnHomepage = _webApiSettings.ShowBestsellersOnHomepage && _webApiSettings.NumberOfBestsellersOnHomepage > 0,
                ShowHomepageCategoryProducts = _webApiSettings.ShowHomepageCategoryProducts,
                ShowManufacturers = _webApiSettings.ShowManufacturers,
                AndriodForceUpdate = _webApiSettings.AndriodForceUpdate,
                AndroidVersion = _webApiSettings.AndroidVersion,
                PlayStoreUrl = _webApiSettings.PlayStoreUrl,
                IOSForceUpdate = _webApiSettings.IOSForceUpdate,
                IOSVersion = _webApiSettings.IOSVersion,
                AppStoreUrl = _webApiSettings.AppStoreUrl,
                ShowAllVendors = _vendorSettings.VendorsBlockItemsToDisplay > 0,
                LogoUrl = await _pictureService.GetPictureUrlAsync(_webApiSettings.LogoId, _webApiSettings.LogoSize),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
                ShowChangeBaseUrlPanel = _webApiSettings.ShowChangeBaseUrlPanel,
                HasReturnRequests = _orderSettings.ReturnRequestsEnabled &&
                    (await _returnRequestService.SearchReturnRequestsAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                    (await _workContext.GetCurrentCustomerAsync()).Id, pageIndex: 0, pageSize: 1)).Any(),
                HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                AllowCustomersToUploadAvatars = _customerSettings.AllowCustomersToUploadAvatars,
                AvatarMaximumSizeBytes = _customerSettings.AvatarMaximumSizeBytes,
                HideBackInStockSubscriptionsTab = _customerSettings.HideBackInStockSubscriptionsTab,
                StoreClosed = _storeInformationSettings.StoreClosed,
                ShoppingCartEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist),
                AllowCustomersToDeleteAccount = _webApiSettings.AllowCustomersToDeleteAccount
            };

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
            model.TotalShoppingCartProducts = cart.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Sum(item => item.Quantity);
            model.TotalWishListProducts = cart.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).Sum(item => item.Quantity);

            var language = await EnsureLanguageIsActive(await _workContext.GetWorkingLanguageAsync());
            model.Rtl = language.Rtl;
            model.StringResources = await _commonApiModelFactory.GetStringRsourcesAsync(language.Id);

            model.LanguageNavSelector = await _commonModelFactory.PrepareLanguageSelectorModelAsync();
            model.CurrencyNavSelector = await _commonModelFactory.PrepareCurrencySelectorModelAsync();

            response.Data = model;

            return Ok(response);
        }

        [HttpGet("manufacturers")]
        public virtual async Task<IActionResult> Manufacturers()
        {
            var response = new GenericResponseModel<List<ManufacturerModel>>
            {
                Data = (await _catalogApiModelFactory.PrepareHomepageManufacturerModelsAsync()).ToList()
            };

            return Ok(response);
        }

        [HttpGet("categorytree")]
        public virtual async Task<IActionResult> CategoryTree()
        {
            var response = new GenericResponseModel<List<CategoryTreeModel>>();
            var model = await _catalogApiModelFactory.PrepareCategoryTreeModelAsync();
            response.Data = model.ToList();
            return Ok(response);
        }

        [HttpGet("featureproducts")]
        public virtual async Task<IActionResult> FeatureProducts(int? productThumbPictureSize)
        {
            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            var model = new List<ProductOverviewModel>();
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
            //ACL and store mapping
            products = await products.WhereAwait(async p => (await _aclService.AuthorizeAsync(p)) && (await _storeMappingService.AuthorizeAsync(p))).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            products = products.Where(p => p.VisibleIndividually).ToList();

            model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
            response.Data = model;
            return Ok(response);
        }

        [HttpGet("bestsellerproducts")]
        public virtual async Task<IActionResult> BestSellerproducts(int? productThumbPictureSize)
        {
            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            var model = new List<ProductOverviewModel>();

            if (!_webApiSettings.ShowBestsellersOnHomepage || _webApiSettings.NumberOfBestsellersOnHomepage <= 0)
            {
                response.Data = model;
                return BadRequest(response);
            }

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ApiModelCacheDefaults.HomepageBestsellersIdsKey,
                       (await _storeContext.GetCurrentStoreAsync()).Id);

            //load and cache report
            var report = await _cacheManager.GetAsync(cacheKey,
                async () => (await _orderReportService.BestSellersReportAsync(
                        storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                        pageSize: _webApiSettings.NumberOfBestsellersOnHomepage)).ToList());

            //load products
            var products = await _productService.GetProductsByIdsAsync(report.Select(x => x.ProductId).ToArray());
            //ACL and store mapping
            products = await products.WhereAwait(async p => (await _aclService.AuthorizeAsync(p)) && (await _storeMappingService.AuthorizeAsync(p))).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();
            response.Data = model;
            return Ok(response);
        }

        [HttpGet("homepagecategorieswithproducts")]
        public virtual async Task<IActionResult> HomePageCategoriesWithProducts()
        {
            var response = new GenericResponseModel<List<HomepageCategoryModel>>();
            var model = await _catalogApiModelFactory.PrepareHomepageCategoriesWithProductsModelAsync();
            response.Data = model.ToList();
            return Ok(response);
        }

        #endregion
    }
}
