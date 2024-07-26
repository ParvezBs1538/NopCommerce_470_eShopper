using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Payments.AfterpayPayment.Controllers
{
    public class OverridenShoppingCartController : ShoppingCartController
    {
        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICurrencyService _currencyService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;

        public OverridenShoppingCartController(CaptchaSettings captchaSettings,
            CustomerSettings customerSettings, ICacheKeyService cacheKeyService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings) : base(captchaSettings,
                customerSettings,
                cacheKeyService,
                checkoutAttributeParser,
                checkoutAttributeService,
                currencyService,
                customerActivityService,
                customerService,
                discountService,
                downloadService,
                genericAttributeService,
                giftCardService,
                localizationService,
                fileProvider,
                notificationService,
                permissionService,
                pictureService,
                priceFormatter,
                productAttributeParser,
                productAttributeService,
                productService,
                shippingService,
                shoppingCartModelFactory,
                shoppingCartService,
                staticCacheManager,
                storeContext,
                taxService,
                urlRecordService,
                webHelper,
                workContext,
                workflowMessageService,
                mediaSettings,
                orderSettings,
                shoppingCartSettings)
        {
            _cacheKeyService = cacheKeyService;
            _currencyService = currencyService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public override IActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            bool loadPicture, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var errors = new List<string>();
            var attributeXml = _productAttributeParser.ParseProductAttributes(product, form, errors);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                _productAttributeParser.ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //sku, mpn, gtin
            var sku = _productService.FormatSku(product, attributeXml);
            var mpn = _productService.FormatMpn(product, attributeXml);
            var gtin = _productService.FormatGtin(product, attributeXml);

            // calculating weight adjustment
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributeXml);
            var totalWeight = product.BasepriceAmount;

            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueType)
                {
                    case AttributeValueType.Simple:
                        //simple attribute
                        totalWeight += attributeValue.WeightAdjustment;
                        break;
                    case AttributeValueType.AssociatedToProduct:
                        //bundled product
                        var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                        if (associatedProduct != null)
                            totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                        break;
                }
            }

            //price
            var price = string.Empty;
            decimal priceValue = 0;
            //base price
            var basepricepangv = string.Empty;
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice)
            {
                //we do not calculate price of "customer enters price" option is enabled
                var finalPrice = _shoppingCartService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out var _, out _);
                var finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPrice, out var _);
                var finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                basepricepangv = _priceFormatter.FormatBasePrice(product, finalPriceWithDiscountBase, totalWeight);
                priceValue = finalPriceWithDiscount;
            }

            //stock
            var stockAvailability = _productService.FormatStockMessage(product, attributeXml);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = string.Empty;
            var pictureDefaultSizeUrl = string.Empty;
            if (loadPicture)
            {
                //first, try to get product attribute combination picture
                var pictureId = _productAttributeParser.FindProductAttributeCombination(product, attributeXml)?.PictureId ?? 0;

                //then, let's see whether we have attribute values with pictures
                if (pictureId == 0)
                {
                    pictureId = _productAttributeParser.ParseProductAttributeValues(attributeXml)
                        .FirstOrDefault(attributeValue => attributeValue.PictureId > 0)?.PictureId ?? 0;
                }

                if (pictureId > 0)
                {
                    var productAttributePictureCacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore);
                    var pictureModel = _staticCacheManager.Get(productAttributePictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(pictureId);
                        return picture == null ? new PictureModel() : new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                            ImageUrl = _pictureService.GetPictureUrl(ref picture, _mediaSettings.ProductDetailsPictureSize)
                        };
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }
            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            {
                isFreeShipping = _productAttributeParser.ParseProductAttributeValues(attributeXml)
                    .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    .Select(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId))
                    .All(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
            }
            var afterpayInstallmentText = RenderViewComponentToString("AfterpayInstallment", new { additionalData = priceValue });
            return Json(new
            {
                productId,
                gtin,
                mpn,
                sku,
                price,
                afterpayInstallmentText,
                basepricepangv,
                stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl,
                pictureDefaultSizeUrl,
                isFreeShipping,
                message = errors.Any() ? errors.ToArray() : null
            });
        }
    }
}
