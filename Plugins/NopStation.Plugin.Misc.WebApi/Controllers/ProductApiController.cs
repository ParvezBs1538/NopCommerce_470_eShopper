using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/product")]
    public class ProductApiController : BaseApiController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IAclService _aclService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IOrderReportService _orderReportService;
        private readonly ICustomerService _customerService;
        private readonly IReviewTypeService _reviewTypeService;
        //private readonly ICacheKeyService _cacheKeyService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeApiParser _productAttributeApiParser;
        private readonly WebApiSettings _webApiSettings;
        private readonly IProductApiService _productApiService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Ctor

        public ProductApiController(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            ICatalogModelFactory catalogModelFactory,
            IAclService aclService,
            ICompareProductsService compareProductsService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            ShoppingCartSettings shoppingCartSettings,
            IStaticCacheManager cacheManager,
            IOrderReportService orderReportService,
            ICustomerService customerService,
            IReviewTypeService reviewTypeService,
            //ICacheKeyService cacheKeyService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IProductAttributeParser productAttributeParser,
            IProductAttributeApiParser productAttributeApiParser,
            WebApiSettings webApiSettings,
            IProductApiService productApiService,
            IHtmlFormatter htmlFormatter,
            ShippingSettings shippingSettings)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _catalogModelFactory = catalogModelFactory;
            _aclService = aclService;
            _compareProductsService = compareProductsService;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _cacheManager = cacheManager;
            _orderReportService = orderReportService;
            _customerService = customerService;
            _reviewTypeService = reviewTypeService;
            //_cacheKeyService = cacheKeyService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _productAttributeParser = productAttributeParser;
            _productAttributeApiParser = productAttributeApiParser;
            _webApiSettings = webApiSettings;
            _productApiService = productApiService;
            _htmlFormatter = htmlFormatter;
            _shippingSettings = shippingSettings;
        }

        #endregion

        #region Utilities

        #endregion

        #region Product details page

        [HttpGet("productdetails/{productId}/{updatecartitemid?}")]
        public virtual async Task<IActionResult> ProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Product.ProductNotFound"));

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return NotFound();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return BadRequest();

                return Redirect($"api/product/productdetails/{parentGroupedProduct.Id}");
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return Redirect($"api/product/productdetails/{product.Id}");
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return Redirect($"api/product/productdetails/{product.Id}");
                }
            }

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);

            return OkWrap(model);
        }

        [HttpGet("getproductbybarcode/{productCode}")]
        public virtual async Task<IActionResult> ProductDetails(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Product.ProductByBarCode.ProductNotFound"));

            Product product;
            if (_webApiSettings.ProductBarcodeScanKeyId == (int)BarcodeScanKeyType.Sku)
                product = await _productService.GetProductBySkuAsync(productCode);
            else if (_webApiSettings.ProductBarcodeScanKeyId == (int)BarcodeScanKeyType.Gtin)
                product = await _productApiService.GetProductByGtinAsync(productCode);
            else
            {
                int.TryParse(productCode, out var productId);
                product = await _productService.GetProductByIdAsync(productId);
            }

            if (product == null || product.Deleted)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Product.ProductByBarCode.ProductNotFound"));

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return NotFound();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return BadRequest();

                return Redirect($"api/product/productdetails/{parentGroupedProduct.Id}");
            }

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, null, false);

            return OkWrap(model);
        }

        [HttpPost("estimateshipping")]
        public virtual async Task<IActionResult> EstimateShipping([FromBody] BaseQueryModel<ProductDetailsModel.ProductEstimateShippingModel> queryModel)
        {
            var model = queryModel.Data;
            var form = queryModel.FormValues.ToNameValueCollection();
            if (model == null)
                model = new ProductDetailsModel.ProductEstimateShippingModel();

            var errors = new List<string>();
            if (!_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));

            if (_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.City))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

            if (errors.Count > 0)
                return BadRequest(null, errors);

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted)
            {
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShippingPopUp.Product.IsNotFound"));
                return BadRequest(null, errors);
            }

            var wrappedProduct = new ShoppingCartItem()
            {
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                ProductId = product.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            var addToCartWarnings = new List<string>();
            //customer entered price
            wrappedProduct.CustomerEnteredPrice = await _productAttributeApiParser.ParseCustomerEnteredPriceAsync(product, form);

            //entered quantity
            wrappedProduct.Quantity = _productAttributeApiParser.ParseEnteredQuantity(product, form);

            //product and gift card attributes
            wrappedProduct.AttributesXml = await _productAttributeApiParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

            //rental attributes
            _productAttributeApiParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);
            wrappedProduct.RentalStartDateUtc = rentalStartDate;
            wrappedProduct.RentalEndDateUtc = rentalEndDate;

            var response = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(new[] { wrappedProduct }, model, false);

            return OkWrap(response);
        }

        [HttpGet("relatedproducts/{productId}/{productThumbPictureSize?}")]
        public async Task<IActionResult> RelatedProducts(int productId, int? productThumbPictureSize)
        {
            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            //load and cache report
            var productsRelatedIdsCacheKey = _cacheManager.PrepareKeyForDefaultCache(ApiModelCacheDefaults.ProductsRelatedIdsKey,
                       productId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var productIds = await _cacheManager.GetAsync(productsRelatedIdsCacheKey,
                async () => (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray());

            //load products
            var products = await _productService.GetProductsByIdsAsync(productIds);
            //ACL and store mapping
            products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Ok(response);

            response.Data = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            return Ok(response);
        }

        [HttpGet("productsalsopurchased/{productId}/{productThumbPictureSize?}")]
        public async Task<IActionResult> ProductsAlsoPurchased(int productId, int? productThumbPictureSize)
        {
            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
                return Ok(response);

            //load and cache report
            var productsAlsoPurchasedIdsCacheKey = _cacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductsAlsoPurchasedIdsKey,
                        productId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var productIds = await _cacheManager.GetAsync(productsAlsoPurchasedIdsCacheKey,
                async () => await _orderReportService.GetAlsoPurchasedProductsIdsAsync((await _storeContext.GetCurrentStoreAsync()).Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber)
            );

            //load products
            var products = await _productService.GetProductsByIdsAsync(productIds);
            //ACL and store mapping
            products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            if (!products.Any())
                return Ok(response);

            response.Data = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            return Ok(response);
        }

        [HttpGet("crosssellproducts/{productThumbPictureSize?}")]
        public async Task<IActionResult> CrossSellProducts(int? productThumbPictureSize)
        {
            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var products = await _productService.GetCrossSellProductsByShoppingCartAsync(cart, _shoppingCartSettings.CrossSellsNumber);
            //ACL and store mapping
            products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Ok(response);

            //Cross-sell products are displayed on the shopping cart page.
            //We know that the entire shopping cart page is not refresh
            //even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
            //That's why we force page refresh (redirect) in this case
            response.Data = (await _productModelFactory.PrepareProductOverviewModelsAsync(products,
                    productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)).ToList();

            return Ok(response);
        }

        #endregion

        #region Recently viewed products

        [HttpGet("recentlyviewedproducts")]
        public virtual async Task<IActionResult> RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return BadRequest();

            var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(_catalogSettings.RecentlyViewedProductsNumber);

            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            response.Data = new List<ProductOverviewModel>();
            response.Data.AddRange(await _productModelFactory.PrepareProductOverviewModelsAsync(products));

            return Ok(response);
        }

        #endregion

        #region New (recently added) products page

        [HttpGet("newproducts")]
        public virtual async Task<IActionResult> NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return BadRequest();

            var products = await _productService.GetProductsMarkedAsNewAsync((await _storeContext.GetCurrentStoreAsync()).Id);

            var response = new GenericResponseModel<List<ProductOverviewModel>>();
            response.Data = new List<ProductOverviewModel>();
            response.Data.AddRange(await _productModelFactory.PrepareProductOverviewModelsAsync(products));

            return Ok(response);
        }

        #endregion

        #region Product reviews

        [HttpGet("productreviews/{productId}")]
        public virtual async Task<IActionResult> ProductReviews(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Product.ProductNotFound"));

            if (!product.AllowCustomerReviews)
                return BadRequest();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var errors = new List<string>();

            var model = await _productModelFactory.PrepareProductReviewsModelAsync(product);

            //only registered users can leave reviews
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                errors.Add(await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
            {
                var hasCompletedOrders = (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                    productId: productId,
                    osIds: new List<int> { (int)OrderStatus.Complete },
                    pageSize: 1)).Any();
                if (!hasCompletedOrders)
                    errors.Add(await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
            }

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            model.AddProductReview.CanAddNewReview = await _productService.CanAddReviewAsync(product.Id, (await _storeContext.GetCurrentStoreAsync()).Id);

            //default value for all additional review types
            if (model.ReviewTypeList.Count > 0)
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                {
                    additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;
                }

            return OkWrap(model, errors: errors);
        }

        [HttpPost("productreviewsadd/{productId:min(0)}")]
        public virtual async Task<IActionResult> ProductReviewsAdd(int productId, [FromBody] BaseQueryModel<ProductReviewsModel> queryModel)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Product.ProductNotFound"));

            if (!product.AllowCustomerReviews)
                return BadRequest();

            var model = queryModel.Data;
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
            {
                var hasCompletedOrders = (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                    productId: productId,
                    osIds: new List<int> { (int)OrderStatus.Complete },
                    pageSize: 1)).Any();
                if (!hasCompletedOrders)
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
            }

            if (ModelState.IsValid)
            {
                //save review
                var rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;
                var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = customer.Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                };

                await _productService.InsertProductReviewAsync(productReview);

                //add product review and review type mapping                
                foreach (var additionalReview in model.AddAdditionalProductReviewList)
                {
                    var additionalProductReview = new ProductReviewReviewTypeMapping
                    {
                        ProductReviewId = productReview.Id,
                        ReviewTypeId = additionalReview.ReviewTypeId,
                        Rating = additionalReview.Rating
                    };
                    await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
                }

                //update product totals
                await _productService.UpdateProductReviewTotalsAsync(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    await _workflowMessageService.SendProductReviewStoreOwnerNotificationMessageAsync(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

                //raise event
                if (productReview.IsApproved)
                    await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

                model = await _productModelFactory.PrepareProductReviewsModelAsync(product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                var message = !isApproved ? await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving") :
                    await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded");

                return OkWrap(model, message);
            }

            //If we got this far, something failed, redisplay form
            model = await _productModelFactory.PrepareProductReviewsModelAsync(product);
            return BadRequestWrap(model, ModelState);
        }

        [HttpPost("setproductreviewhelpfulness/{productReviewId:min(0)}")]
        public virtual async Task<IActionResult> SetProductReviewHelpfulness(int productReviewId, [FromBody] BaseQueryModel<object> queryModel)
        {
            var productReview = await _productService.GetProductReviewByIdAsync(productReviewId);
            if (productReview == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Product.ProductReviewNotFound"));

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            var washelpful = form["washelpful"] != null ? bool.Parse(form["washelpful"]) : true;

            var response = new ProductReviewHelpfulnessModel();
            var errors = new List<string>();

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                response.HelpfulYesTotal = productReview.HelpfulYesTotal;
                response.HelpfulNoTotal = productReview.HelpfulNoTotal;

                errors.Add(await _localizationService.GetResourceAsync("Reviews.Helpfulness.OnlyRegistered"));
                return BadRequestWrap(response, errors: errors);
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
            {
                response.HelpfulYesTotal = productReview.HelpfulYesTotal;
                response.HelpfulNoTotal = productReview.HelpfulNoTotal;

                errors.Add(await _localizationService.GetResourceAsync("Reviews.Helpfulness.YourOwnReview"));
                return BadRequestWrap(response, errors: errors);
            }

            await _productService.SetProductReviewHelpfulnessAsync(productReview, washelpful);

            //new totals
            await _productService.UpdateProductReviewHelpfulnessTotalsAsync(productReview);

            response.HelpfulYesTotal = productReview.HelpfulYesTotal;
            response.HelpfulNoTotal = productReview.HelpfulNoTotal;

            return OkWrap(response, await _localizationService.GetResourceAsync("Reviews.Helpfulness.SuccessfullyVoted"));
        }

        [HttpGet("productreviews")]
        public virtual async Task<IActionResult> CustomerProductReviews(int? pageNumber)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
                return BadRequest();

            var response = await _productModelFactory.PrepareCustomerProductReviewsModelAsync(pageNumber);
            return OkWrap(response);
        }

        #endregion

        #region Email a friend

        [HttpGet("productemailafriend/{productId:min(0)}")]
        public virtual async Task<IActionResult> ProductEmailAFriend(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return NotFound();

            if (!product.AllowCustomerReviews)
                return BadRequest();

            var response = await _productModelFactory.PrepareProductEmailAFriendModelAsync(new ProductEmailAFriendModel(), product, false);
            return OkWrap(response);
        }

        [HttpPost("productemailafriendsend")]
        public virtual async Task<IActionResult> ProductEmailAFriendSend([FromBody] BaseQueryModel<ProductEmailAFriendModel> queryModel)
        {
            var model = queryModel.Data;

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted || !product.Published)
                return NotFound();

            if (!product.AllowCustomerReviews)
                return BadRequest();

            //check whether the current customer is guest and ia allowed to email a friend
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            //var response = new GenericResponseModel<ProductEmailAFriendModel>();
            if (ModelState.IsValid)
            {
                //email
                await _workflowMessageService.SendProductEmailAFriendMessageAsync(await _workContext.GetCurrentCustomerAsync(),
                        (await _workContext.GetWorkingLanguageAsync()).Id, product,
                        model.YourEmailAddress, model.FriendEmail,
                        _htmlFormatter.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, true);
                model.SuccessfullySent = true;

                return OkWrap(model, await _localizationService.GetResourceAsync("Products.EmailAFriend.SuccessfullySent"));
            }

            //If we got this far, something failed, redisplay form
            model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, true);
            return BadRequestWrap(model, ModelState);
        }

        #endregion

        #region Comparing products

        [HttpPost("compareproducts/add/{productId:min(0)}")]
        public virtual async Task<IActionResult> AddProductToCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return NotFound();

            if (!product.AllowCustomerReviews)
                return BadRequest();

            if (!_catalogSettings.CompareProductsEnabled)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Catalog.ProductComparisonDisabled"));

            await _compareProductsService.AddProductToCompareListAsync(productId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddToCompareList",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToCompareList"), product.Name), product);

            return Ok(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToCompareList"));
        }

        [HttpGet("compareproducts/remove/{productId}")]
        public virtual async Task<IActionResult> RemoveProductFromCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            if (!_catalogSettings.CompareProductsEnabled)
                return BadRequest();

            await _compareProductsService.RemoveProductFromCompareListAsync(productId);

            return Ok();
        }

        [HttpGet("compareproducts")]
        public virtual async Task<IActionResult> CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return BadRequest();

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = await _compareProductsService.GetComparedProductsAsync();

            //ACL and store mapping
            products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            //prepare model
            (await _productModelFactory.PrepareProductOverviewModelsAsync(products, prepareSpecificationAttributes: true))
                .ToList()
                .ForEach(model.Products.Add);

            return OkWrap(model);
        }

        [HttpGet("clearcomparelist")]
        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return BadRequest();

            _compareProductsService.ClearCompareProducts();

            return Ok();
        }

        #endregion
    }
}
