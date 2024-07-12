using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Models.ShoppingCart;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/shoppingcart")]
    public partial class ShoppingCartApiController : BaseApiController
    {
        #region Fields

        protected readonly CustomerSettings _customerSettings;
        protected readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        protected readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
        protected readonly ICurrencyService _currencyService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ICustomerService _customerService;
        protected readonly IDiscountService _discountService;
        protected readonly IDownloadService _downloadService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IGiftCardService _giftCardService;
        protected readonly IHtmlFormatter _htmlFormatter;
        protected readonly ILocalizationService _localizationService;
        protected readonly INopFileProvider _fileProvider;
        protected readonly INopUrlHelper _nopUrlHelper;
        protected readonly INotificationService _notificationService;
        protected readonly IPermissionService _permissionService;
        protected readonly IPictureService _pictureService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IProductService _productService;
        protected readonly IShippingService _shippingService;
        protected readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreContext _storeContext;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly ITaxService _taxService;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IWebHelper _webHelper;
        protected readonly IWorkContext _workContext;
        protected readonly IWorkflowMessageService _workflowMessageService;
        protected readonly MediaSettings _mediaSettings;
        protected readonly OrderSettings _orderSettings;
        protected readonly ShoppingCartSettings _shoppingCartSettings;
        protected readonly ShippingSettings _shippingSettings;
        private static readonly char[] _separator = [','];

        #endregion

        #region Ctor

        public ShoppingCartApiController(CustomerSettings customerSettings,
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            INopUrlHelper nopUrlHelper,
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
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings)
        {
            _customerSettings = customerSettings;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _nopUrlHelper = nopUrlHelper;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
        }

        #endregion

        #region Utilities

        protected async Task<ShoppingCartApiModel> PrepareShoppingCartApiModelAsync(ShoppingCartModel cartModel, IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new ShoppingCartApiModel
            {
                Cart = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(cartModel, cart),
                OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, isEditable),
                SelectedCheckoutAttributes = await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync()
            };

            if (_shippingSettings.EstimateShippingCartPageEnabled)
                model.EstimateShipping = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(cart);

            model.AnonymousPermissed = _orderSettings.AnonymousCheckoutAllowed
                                     && _customerSettings.UserRegistrationType == UserRegistrationType.Disabled;

            return model;
        }

        protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, NameValueCollection form)
        {
            ArgumentNullException.ThrowIfNull(cart);

            ArgumentNullException.ThrowIfNull(form);

            var attributesXml = string.Empty;
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var checkoutAttributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);
            foreach (var attribute in checkoutAttributes)
            {
                var controlId = $"checkout_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _checkoutAttributeService.GetAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                         .Where(v => v.IsPreSelected)
                                         .Select(v => v.Id)
                                         .ToList())
                            {
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }

                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                            }
                            catch
                            {
                                // ignored
                            }

                            if (selectedDate.HasValue)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:
                        {
                            _ = Guid.TryParse(form[controlId], out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParser.RemoveAttribute(attributesXml, attribute.Id);
            }

            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, store.Id);
        }

        protected virtual async Task<string> ParseProductAttributes(Product product, NameValueCollection form, List<string> errors)
        {
            //product attributes
            var attributesXml = await GetProductAttributesXml(product, form, errors);

            //gift cards
            AddGiftCardsAttributesXml(product, form, ref attributesXml);

            return attributesXml;
        }

        protected virtual void ParseRentalDates(Product product, NameValueCollection form,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            var startControlId = $"rental_start_date_{product.Id}";
            var endControlId = $"rental_end_date_{product.Id}";
            var ctrlStartDate = form[startControlId];
            var ctrlEndDate = form[endControlId];
            try
            {
                //currently we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        protected virtual void AddGiftCardsAttributesXml(Product product, NameValueCollection form, ref string attributesXml)
        {
            if (!product.IsGiftCard)
                return;

            var recipientName = "";
            var recipientEmail = "";
            var senderName = "";
            var senderEmail = "";
            var giftCardMessage = "";
            foreach (string formKey in form.Keys)
            {
                if (formKey.Equals($"giftcard_{product.Id}.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientEmail = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.SenderName", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderEmail = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.Message", StringComparison.InvariantCultureIgnoreCase))
                {
                    giftCardMessage = form[formKey];
                }
            }

            attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml, recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
        }

        protected virtual async Task<string> GetProductAttributesXml(Product product, NameValueCollection form, List<string> errors)
        {
            var attributesXml = string.Empty;
            var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                {
                                    //get quantity entered by customer
                                    var quantity = 1;
                                    var quantityStr = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                    if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                        (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                        errors.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        var quantityStr = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{item}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));

                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                            }
                            catch
                            {
                            }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(form[controlId], out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }

        protected virtual async Task SaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
           ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
           DateTime? rentalEndDate, int quantity)
        {
            if (updatecartitem == null)
            {
                //add to the cart
                addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                    product, cartType, (await _storeContext.GetCurrentStoreAsync()).Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true));
            }
            else
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), updatecartitem.ShoppingCartType, (await _storeContext.GetCurrentStoreAsync()).Id);

                var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                    cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's some other shopping cart item
                    otherCartItemWithSameParameters = null;
                }
                //update existing item
                addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
                }
            }
        }

        #endregion

        #region Shopping cart

        [HttpPost("addproducttocart/catalog/{productId:min(0)}/{shoppingCartTypeId:min(0)}")]
        public virtual async Task<IActionResult> AddProductToCart_Catalog(int productId, int shoppingCartTypeId, [FromBody] BaseQueryModel<string> queryModel)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.AddToCart.ProductNotFound"));

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

            var quantity = 1;
            foreach (string formKey in form.Keys)
                if (formKey.Equals($"quantity", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }

            var forceredirection = false;
            foreach (string formKey in form.Keys)
                if (formKey.Equals($"forceredirection", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool.TryParse(form[formKey], out forceredirection);
                    break;
                }

            var response = new GenericResponseModel<AddToCartResponseModel>
            {
                Data = new AddToCartResponseModel()
            };

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            if (product.CustomerEntersPrice)
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            if (product.IsRental)
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            //creating XML for "read-only checkboxes" attributes
            var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
            {
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }
                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType, (await _storeContext.GetCurrentStoreAsync()).Id);
            var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = await _shoppingCartService
                .GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), cartType,
                product, (await _storeContext.GetCurrentStoreAsync()).Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
            if (addToCartWarnings.Any())
            {
                response.ErrorList.AddRange(addToCartWarnings);
                return BadRequest(response);
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: (await _workContext.GetCurrentCustomerAsync()),
                product: product,
                shoppingCartType: cartType,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                attributesXml: attXml,
                quantity: quantity);
            if (addToCartWarnings.Any())
            {
                response.Data.RedirectToDetailsPage = true;
                return Ok(response);
            }

            var updatedCart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new AddToCartResponseModel()
            {
                TotalShoppingCartProducts = updatedCart.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Sum(item => item.Quantity),
                TotalWishListProducts = updatedCart.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).Sum(item => item.Quantity)
            };
            response.Data = model;

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            response.Data.RedirectToWishListPage = true;
                            return Ok(response);
                        }
                        break;
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            response.Data.RedirectToShoppingCartPage = true;
                            return Ok(response);
                        }
                        break;
                    }
            }

            return Ok(response);
        }

        [HttpPost("addproducttocart/details/{productId:min(0)}/{shoppingCartTypeId:min(0)}")]
        public virtual async Task<IActionResult> AddProductToCart_Details(int productId, int shoppingCartTypeId, [FromBody] BaseQueryModel<string> queryModel)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.AddToCart.ProductNotFound"));

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.AddToCart.SimpleProductOnly"));

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            //update existing shopping cart item
            var updatecartitemid = 0;
            foreach (string formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                //search with the same cart type as specified
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType)shoppingCartTypeId, (await _storeContext.GetCurrentStoreAsync()).Id);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.AddToCart.NotMatchingWithCartItem"));
            }

            //customer entered price
            var customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
            {
                foreach (string formKey in form.Keys)
                {
                    if (formKey.Equals($"addtocart_{productId}.CustomerEnteredPrice", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (decimal.TryParse(form[formKey], out var customerEnteredPrice))
                            customerEnteredPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(customerEnteredPrice, await _workContext.GetWorkingCurrencyAsync());
                        break;
                    }
                }
            }

            var quantity = 1;
            foreach (string formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }

            var addToCartWarnings = new List<string>();

            //product and gift card attributes
            var attributes = await ParseProductAttributes(product, form, addToCartWarnings);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                updatecartitem.ShoppingCartType;

            await SaveItemAsync(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity);

            var response = new GenericResponseModel<AddToCartResponseModel>();
            if (addToCartWarnings.Any())
            {
                response.ErrorList.AddRange(addToCartWarnings);
                return BadRequest(response);
            }

            var updatedCart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new AddToCartResponseModel()
            {
                TotalShoppingCartProducts = updatedCart.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Sum(item => item.Quantity),
                TotalWishListProducts = updatedCart.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).Sum(item => item.Quantity)
            };
            response.Data = model;

            response.Message = (ShoppingCartType)shoppingCartTypeId == ShoppingCartType.ShoppingCart ?
                await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart") :
                await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishList");

            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            response.Data.RedirectToWishListPage = true;
                            return Ok(response);
                        }
                        break;
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            response.Data.RedirectToShoppingCartPage = true;
                            return Ok(response);
                        }
                        break;
                    }
            }

            return Ok(response);
        }

        [HttpPost("productattributechange/{productId:min(0)}")]
        public virtual async Task<IActionResult> ProductDetails_AttributeChange(int productId, [FromBody] BaseQueryModel<string> queryModel)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            var validateAttributeConditions = form["validateAttributeConditions"] != null ? bool.Parse(form["validateAttributeConditions"]) : false;
            var loadPicture = form["loadPicture"] != null ? bool.Parse(form["loadPicture"]) : false;

            var errors = new List<string>();
            var attributeXml = await ParseProductAttributes(product, form, errors);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //sku, mpn, gtin
            var sku = await _productService.FormatSkuAsync(product, attributeXml);
            var mpn = await _productService.FormatMpnAsync(product, attributeXml);
            var gtin = await _productService.FormatGtinAsync(product, attributeXml);

            // calculating weight adjustment
            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);
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
                        var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                        if (associatedProduct != null)
                            totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                        break;
                }
            }

            //price
            var price = "";
            //base price
            var basepricepangv = "";
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                //we do not calculate price of "customer enters price" option is enabled
                var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                    currentCustomer,
                    currentStore,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate, true);
                var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, finalPrice);
                var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                basepricepangv = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase, totalWeight);
            }

            //stock
            var stockAvailability = await _productService.FormatStockMessageAsync(product, attributeXml);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
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
            var pictureIds = new List<int>();
            if (loadPicture)
            {
                //first, try to get product attribute combination picture
                var pictureId = 0;
                var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml);
                if (combination != null)
                {
                    var combinationPictures = await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id);
                    pictureIds = combinationPictures.Select(cp => cp.PictureId).ToList();
                    pictureId = combinationPictures.FirstOrDefault()?.PictureId ?? 0;
                }

                //then, let's see whether we have attribute values with pictures
                if (pictureId == 0)
                {
                    var valuePictures = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                        .SelectManyAwait(async attributeValue => await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id))
                        .ToListAsync();
                    pictureIds = valuePictures.Select(vp => vp.PictureId).ToList();
                    pictureId = valuePictures.FirstOrDefault()?.PictureId ?? 0;
                }

                if (pictureId > 0)
                {
                    var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                    var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                        string fullSizeImageUrl, imageUrl;

                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);

                        return picture == null ? new PictureModel() : new PictureModel
                        {
                            FullSizeImageUrl = fullSizeImageUrl,
                            ImageUrl = imageUrl
                        };
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }
            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            {
                isFreeShipping = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                    .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    .SelectAwait(async attributeValue => await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId))
                    .AllAsync(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
            }

            var response = new GenericResponseModel<CartAttributeChangeModel>
            {
                Data = new CartAttributeChangeModel()
                {
                    ProductId = productId,
                    Gtin = gtin,
                    Mpn = mpn,
                    Sku = sku,
                    Price = price,
                    BasePricePangv = basepricepangv,
                    StockAvailability = stockAvailability,
                    EnabledAttributeMappingIds = enabledAttributeMappingIds.ToArray(),
                    DisabledAttributeMappingIds = disabledAttributeMappingIds.ToArray(),
                    PictureFullSizeUrl = pictureFullSizeUrl,
                    PictureDefaultSizeUrl = pictureDefaultSizeUrl,
                    PictureIds = pictureIds,
                    IsFreeShipping = isFreeShipping
                },
                ErrorList = errors
            };

            return Ok(response);
        }

        [HttpPost("checkoutattributechange")]
        public virtual async Task<IActionResult> CheckoutAttributeChange([FromBody] BaseQueryModel<string> queryModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            await _productService.GetProductsByIdsAsync(cart.Select(sci => sci.ProductId).ToArray());

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            var isEditable = form["isEditable"] != null ? bool.Parse(form["isEditable"]) : false;

            await ParseAndSaveCheckoutAttributesAsync(cart, form);
            var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);

            //conditions
            var enabledAttributeIds = new List<int>();
            var disabledAttributeIds = new List<int>();
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var attributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }

            return Ok(new CheckoutAttributeChangeModel
            {
                Cart = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(new ShoppingCartModel(), cart, isEditable),
                OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, isEditable),
                SelectedCheckoutAttributess = await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync(),
                EnabledAttributeIds = enabledAttributeIds,
                DisabledAttributeIds = disabledAttributeIds
            });
        }

        [HttpPost("uploadfileproductattribute/{attributeId:min(0)}")]
        public virtual async Task<IActionResult> UploadFileProductAttribute(int attributeId)
        {
            var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeId);
            if (attribute == null)
                return NotFound();

            if (attribute.AttributeControlType != AttributeControlType.FileUpload)
                return BadRequest();

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.ShoppingCart.NoFileUploaded"));
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    return BadRequest(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value));
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            var response = new GenericResponseModel<UploadFileModel>
            {
                Data = new UploadFileModel()
                {
                    DownloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                    DownloadGuid = download.DownloadGuid,
                },
                Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded")
            };

            return Ok(response);
        }

        [HttpPost("uploadfilecheckoutattribute/{attributeId:min(0)}")]
        public virtual async Task<IActionResult> UploadFileCheckoutAttribute(int attributeId)
        {
            var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeId);
            if (attribute == null)
                return NotFound();

            if (attribute.AttributeControlType != AttributeControlType.FileUpload)
                return BadRequest();

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.ShoppingCart.NoFileUploaded"));
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return BadRequest(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value));
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            var response = new GenericResponseModel<UploadFileModel>
            {
                Data = new UploadFileModel()
                {
                    DownloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                    DownloadGuid = download.DownloadGuid,
                },
                Message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded")
            };

            return Ok(response);
        }

        [HttpsRequirement]
        [HttpGet("cart")]
        public virtual async Task<IActionResult> Cart(bool isEditable)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return BadRequest(await _localizationService.GetResourceAsync("Account.Register.Result.Disabled"));

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var response = new GenericResponseModel<ShoppingCartApiModel>
            {
                Data = await PrepareShoppingCartApiModelAsync(new ShoppingCartModel(), cart, isEditable)
            };

            if (!response.Data.Cart.Items.Any())
                response.ErrorList.Add(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            return Ok(response);
        }

        [HttpPost("updatecart")]
        public virtual async Task<IActionResult> UpdateCart([FromBody] BaseQueryModel<string> queryModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return BadRequest(await _localizationService.GetResourceAsync("Account.Register.Result.Disabled"));

            var formValues = queryModel.FormValues;
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var form = formValues == null ? new NameValueCollection() : formValues.ToNameValueCollection();
            var isEditable = form["isEditable"] != null ? bool.Parse(form["isEditable"]) : false;
            //get identifiers of items to remove
            var itemIdsToRemove = new List<int>();
            if (form["removefromcart"] != null)
            {
                itemIdsToRemove = form["removefromcart"]
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                    .Distinct().ToList();
            }

            //get order items with changed quantity
            var itemsWithNewQuantity = cart.Select(item => new
            {
                //try to get a new quantity for the item, set 0 for items to remove
                NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
                Item = item
            }).Where(item => item.NewQuantity != item.Item.Quantity);

            //order cart items
            //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
            var orderedCart = itemsWithNewQuantity
                .OrderByDescending(async cartItem =>
                    (cartItem.NewQuantity < cartItem.Item.Quantity && ((await _productService.GetProductByIdAsync(cartItem.Item.ProductId))?.RequireOtherProducts ?? false)) ||
                    (cartItem.NewQuantity > cartItem.Item.Quantity &&
                        (await cart.AnyAwaitAsync(async item => await _productService.GetProductByIdAsync(item.ProductId) != null &&
                            (await _productService.GetProductByIdAsync(item.ProductId)).RequireOtherProducts &&
                            _productService.ParseRequiredProductIds(await _productService.GetProductByIdAsync(item.ProductId)).Contains(cartItem.Item.ProductId))
                        )
                    )
                 ).ToList();

            //try to update cart items with new quantities and get warnings
            var warnings = await orderedCart.SelectAwait(async cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                    cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                    cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
            }).ToListAsync();

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            //updated cart
            cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            //prepare model
            var model = await PrepareShoppingCartApiModelAsync(new ShoppingCartModel(), cart, isEditable);

            //update current warnings
            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                //find shopping cart item model to display appropriate warnings
                var itemModel = model.Cart.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }

            var response = new GenericResponseModel<ShoppingCartApiModel>
            {
                Data = model
            };

            if (!response.Data.Cart.Items.Any())
                response.ErrorList.Add(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            return Ok(response);
        }

        [HttpPost("applydiscountcoupon")]
        public virtual async Task<IActionResult> ApplyDiscountCoupon([FromBody] BaseQueryModel<string> queryModel)
        {
            //cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart,
                (await _storeContext.GetCurrentStoreAsync()).Id
                );

            var form = queryModel == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            var isEditable = form["isEditable"] != null ? bool.Parse(form["isEditable"]) : false;
            var discountcouponcode = form["discountcouponcode"];

            //trim
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            var cartModel = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                    {
                        var validationResult = await _discountService.ValidateDiscountAsync(discount, await _workContext.GetCurrentCustomerAsync(), new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        await _customerService.ApplyDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discountcouponcode);
                        cartModel.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                        cartModel.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            cartModel.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            cartModel.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    cartModel.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
            }
            else
                //empty coupon code
                cartModel.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));

            var response = new GenericResponseModel<ShoppingCartApiModel>
            {
                Data = await PrepareShoppingCartApiModelAsync(cartModel, cart, isEditable)
            };

            if (!response.Data.Cart.Items.Any())
                response.ErrorList.Add(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            return Ok(response);
        }

        [HttpPost("applygiftcard")]
        public virtual async Task<IActionResult> ApplyGiftCard([FromBody] BaseQueryModel<string> queryModel)
        {
            //cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            var isEditable = form["isEditable"] != null ? bool.Parse(form["isEditable"]) : false;

            var giftcardcouponcode = form["giftcardcouponcode"];
            //trim
            if (giftcardcouponcode != null)
                giftcardcouponcode = giftcardcouponcode.Trim();

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            var cartModel = new ShoppingCartModel();
            if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
            {
                if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftcardcouponcode)).FirstOrDefault();
                    var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                    if (isGiftCardValid)
                    {
                        await _customerService.ApplyGiftCardCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), giftcardcouponcode);
                        cartModel.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.Applied");
                        cartModel.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        cartModel.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        cartModel.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    cartModel.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    cartModel.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                cartModel.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                cartModel.GiftCardBox.IsApplied = false;
            }

            var response = new GenericResponseModel<ShoppingCartApiModel>
            {
                Data = await PrepareShoppingCartApiModelAsync(cartModel, cart, isEditable)
            };

            if (!response.Data.Cart.Items.Any())
                response.ErrorList.Add(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            return Ok(response);
        }

        [HttpPost("cart/estimateshipping")]
        public virtual async Task<IActionResult> GetEstimateShipping([FromBody] BaseQueryModel<EstimateShippingModel> queryModel)
        {
            //var response = new GenericResponseModel<EstimateShippingResultModel>();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            var model = queryModel.Data;
            var errors = new List<string>();
            if (string.IsNullOrEmpty(model.ZipPostalCode))
            {
                errors.Add(await _localizationService.GetResourceAsync("ShoppingCart.EstimateShipping.ZipPostalCode.Required"));
            }

            if (model.CountryId == null || model.CountryId == 0)
            {
                errors.Add(await _localizationService.GetResourceAsync("ShoppingCart.EstimateShipping.Country.Required"));
            }

            var response = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(cart, model, true);

            return OkWrap(response, errors: errors);
        }

        [HttpPost("removediscountcoupon")]
        public virtual async Task<IActionResult> RemoveDiscountCoupon([FromBody] BaseQueryModel<string> queryModel)
        {
            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

            //get discount identifier
            var discountId = 0;
            foreach (string formValue in form.Keys)
                if (formValue.StartsWith("removediscount-", StringComparison.InvariantCultureIgnoreCase))
                    discountId = Convert.ToInt32(formValue.Substring("removediscount-".Length));

            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount != null)
                await _customerService.RemoveDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discount.CouponCode);

            var isEditable = form["isEditable"] != null ? bool.Parse(form["isEditable"]) : false;
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var response = new GenericResponseModel<ShoppingCartApiModel>
            {
                Data = await PrepareShoppingCartApiModelAsync(new ShoppingCartModel(), cart, isEditable)
            };

            if (!response.Data.Cart.Items.Any())
                response.ErrorList.Add(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            return Ok(response);
        }

        [HttpPost("removegiftcardcode")]
        public virtual async Task<IActionResult> RemoveGiftCardCode([FromBody] BaseQueryModel<string> queryModel)
        {
            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

            var isEditable = form["isEditable"] != null ? bool.Parse(form["isEditable"]) : false;

            //get gift card identifier
            var giftCardId = 0;
            foreach (string formValue in form.Keys)
                if (formValue.StartsWith("removegiftcard-", StringComparison.InvariantCultureIgnoreCase))
                    giftCardId = Convert.ToInt32(formValue.Substring("removegiftcard-".Length));
            var gc = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
            if (gc != null)
                await _customerService.RemoveGiftCardCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), gc.GiftCardCouponCode);

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var response = new GenericResponseModel<ShoppingCartApiModel>
            {
                Data = await PrepareShoppingCartApiModelAsync(new ShoppingCartModel(), cart, isEditable)
            };

            if (!response.Data.Cart.Items.Any())
                response.ErrorList.Add(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            return Ok(response);
        }

        #endregion

        #region Wishlist

        [HttpGet("wishlist/{customerguid?}")]
        public virtual async Task<IActionResult> Wishlist(Guid? customerGuid)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return BadRequest();

            var customer = customerGuid.HasValue ?
                await _customerService.GetCustomerByGuidAsync(customerGuid.Value) : await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return NotFound();

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            var response = await _shoppingCartModelFactory.PrepareWishlistModelAsync(new WishlistModel(), cart, !customerGuid.HasValue);

            return OkWrap(response);
        }

        [HttpPost("updatewishlist")]
        public virtual async Task<IActionResult> UpdateWishlist([FromBody] BaseQueryModel<string> queryModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return BadRequest();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

            var allIdsToRemove = form["removefromcart"] != null
                ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                else
                {
                    foreach (string formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out var newQuantity))
                            {
                                var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItemAsync(currentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //updated wishlist
            cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer,
                ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            var response = await _shoppingCartModelFactory.PrepareWishlistModelAsync(new WishlistModel(), cart);

            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = response.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }
            return OkWrap(response);
        }

        [HttpPost("additemstocartfromwishlist")]
        public virtual async Task<IActionResult> AddItemsToCartFromWishlist([FromBody] BaseQueryModel<string> queryModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return BadRequest();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return BadRequest();

            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
            var customerGuid = form["customerGuid"] != null ? Guid.Parse(form["customerGuid"]) : (Guid?)null;

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var pageCustomer = customerGuid.HasValue
                ? await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
                : currentCustomer;
            if (pageCustomer == null)
                return NotFound();

            var pageCart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;

            var allIdsToAdd = form["addtocart"] != null
                ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                : new List<int>();

            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var warnings = await _shoppingCartService.AddToCartAsync(currentCustomer,
                        await _productService.GetProductByIdAsync(sci.ProductId), ShoppingCartType.ShoppingCart,
                        (await _storeContext.GetCurrentStoreAsync()).Id,
                        sci.AttributesXml, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                    if (!warnings.Any())
                        numberOfAddedItems++;
                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)
                    {
                        //let's remove the item from wishlist
                        await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                    }
                    allWarnings.AddRange(warnings);
                }
            }

            var response = new WishlistModel();
            if (numberOfAddedItems > 0)
            {
                if (allWarnings.Any())
                {
                    return BadRequestWrap(response, errors: allWarnings);
                }

                return OkWrap(response, await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart"));
            }
            //no items added. redisplay the wishlist page

            if (allWarnings.Any())
            {
                return BadRequestWrap(response, errors: allWarnings);
            }

            var cart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist,
                (await _storeContext.GetCurrentStoreAsync()).Id);
            response = await _shoppingCartModelFactory.PrepareWishlistModelAsync(new WishlistModel(), cart, !customerGuid.HasValue);

            return OkWrap(response);
        }

        [HttpGet("emailwishlist")]
        public virtual async Task<IActionResult> EmailWishlist()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return BadRequest();

            var response = new WishlistEmailAFriendModel();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
            {
                return BadRequestWrap(response, errors: new List<string>() { await _localizationService.GetResourceAsync("WishList.CartIsEmpty") });
            }

            response = await _shoppingCartModelFactory.PrepareWishlistEmailAFriendModelAsync(new WishlistEmailAFriendModel(), false);
            return OkWrap(response);
        }

        [HttpPost("emailwishlistsend")]
        public virtual async Task<IActionResult> EmailWishlistSend([FromBody] BaseQueryModel<WishlistEmailAFriendModel> queryModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return BadRequest();

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("WishList.CartIsEmpty"));

            //check whether the current customer is guest and ia allowed to email wishlist
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.OnlyRegisteredUsers"));

            var model = queryModel.Data;

            if (ModelState.IsValid)
            {
                //email
                await _workflowMessageService.SendWishlistEmailAFriendMessageAsync(await _workContext.GetCurrentCustomerAsync(),
                        (await _workContext.GetWorkingLanguageAsync()).Id, model.YourEmailAddress,
                        model.FriendEmail, _htmlFormatter.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.SuccessfullySent");

                return OkWrap(model);
            }

            //If we got this far, something failed, redisplay form
            model = await _shoppingCartModelFactory.PrepareWishlistEmailAFriendModelAsync(model, true);

            return BadRequestWrap(model, ModelState);
        }

        #endregion
    }
}