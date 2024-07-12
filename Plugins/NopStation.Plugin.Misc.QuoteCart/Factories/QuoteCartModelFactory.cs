using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Infrastructure;
using NopStation.Plugin.Misc.QuoteCart.Models;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Factories;

public class QuoteCartModelFactory : IQuoteCartModelFactory
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly MediaSettings _mediaSettings;
    private readonly IStoreContext _storeContext;
    private readonly IProductService _productService;
    private readonly VendorSettings _vendorSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IVendorService _vendorService;
    private readonly IWebHelper _webHelper;
    private readonly ITaxService _taxService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly OrderSettings _orderSettings;
    private readonly QuoteCartSettings _quoteCartSettings;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IPictureService _pictureService;
    private readonly IFormAttributeService _formAttributeService;

    #endregion

    #region Ctor

    public QuoteCartModelFactory(
        ICurrencyService currencyService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IPriceFormatter priceFormatter,
        IPictureService pictureService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        QuoteCartSettings quoteCartSettings,
        VendorSettings vendorSettings,
        IFormAttributeService formAttributeService,
        IFormAttributeParser formAttributeParser)
    {
        _workContext = workContext;
        _mediaSettings = mediaSettings;
        _storeContext = storeContext;
        _productService = productService;
        _vendorSettings = vendorSettings;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _urlRecordService = urlRecordService;
        _productAttributeFormatter = productAttributeFormatter;
        _vendorService = vendorService;
        _webHelper = webHelper;
        _taxService = taxService;
        _currencyService = currencyService;
        _customerService = customerService;
        _orderSettings = orderSettings;
        _quoteCartSettings = quoteCartSettings;
        _shoppingCartService = shoppingCartService;
        _staticCacheManager = staticCacheManager;
        _priceFormatter = priceFormatter;
        _pictureService = pictureService;
        _formAttributeService = formAttributeService;
        _formAttributeParser = formAttributeParser;
    }

    #endregion

    #region Methods

    protected virtual async Task<QuoteCartItemModel> PrepareQuoteCartItemModelAsync(IList<QuoteCartItem> cart, QuoteCartItem qci)
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(qci);

        var product = await _productService.GetProductByIdAsync(qci.ProductId);

        var cartItemModel = new QuoteCartItemModel
        {
            Id = qci.Id,
            Sku = await _productService.FormatSkuAsync(product, qci.AttributesXml),
            VendorName = _vendorSettings.ShowVendorOnOrderDetailsPage ? (await _vendorService.GetVendorByProductIdAsync(product.Id))?.Name : string.Empty,
            ProductId = qci.ProductId,
            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            Quantity = qci.Quantity,
            AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, qci.AttributesXml),
        };

        var sci = new ShoppingCartItem
        {
            AttributesXml = qci.AttributesXml,
            StoreId = qci.StoreId,
            ProductId = qci.ProductId,
            Quantity = qci.Quantity,
            ShoppingCartType = ShoppingCartType.ShoppingCart,
            CustomerId = qci.CustomerId,
        };

        //unit prices
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
            cartItemModel.UnitPriceValue = 0;
        }
        else
        {
            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
            cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
            cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
        }
        //subtotal, discount
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
            cartItemModel.SubTotalValue = 0;
        }
        else
        {
            //sub total
            var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
            cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
            cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount;


            //display an applied discount amount
            if (shoppingCartItemDiscountBase > decimal.Zero)
            {
                (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                }
            }
        }

        //rental info
        if (product.IsRental)
        {
            var rentalStartDate = qci.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, qci.RentalStartDateUtc.Value)
                : string.Empty;
            var rentalEndDate = qci.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, qci.RentalEndDateUtc.Value)
                : string.Empty;
            cartItemModel.RentalInfo =
                string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    rentalStartDate, rentalEndDate);
        }

        cartItemModel.Picture = await PrepareCartItemPictureModelAsync(qci, _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);

        return cartItemModel;
    }

    public virtual async Task<CartModel> PrepareQuoteCartModelAsync(CartModel model,
       IList<QuoteCartItem> cart, IList<QuoteForm> quoteForms)
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(model);

        if (cart.Count == 0)
            return model;

        model.CustomerCanEnterPrice = _quoteCartSettings.CustomerCanEnterPrice;

        foreach (var sci in cart)
        {
            var cartItemModel = await PrepareQuoteCartItemModelAsync(cart, sci);
            model.Items.Add(cartItemModel);
        }

        foreach (var form in quoteForms)
        {
            model.Forms.Add(await PrepareQuoteFormModelAsync(new(), form));
        }

        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
        {
            model.CustomProperties.Add("DisplayPrice", "True");
        }
        else
        {
            model.CustomProperties.Add("DisplayPrice", "False");
        }

        return model;
    }

    public async Task<QuoteFormModel> PrepareQuoteFormModelAsync(QuoteFormModel model, QuoteForm quoteForm, string attributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(model);

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (quoteForm != null)
        {
            model = quoteForm.ToModel<QuoteFormModel>();
            model.Title = await _localizationService.GetLocalizedAsync(quoteForm, x => x.Title);
            model.Info = await _localizationService.GetLocalizedAsync(quoteForm, x => x.Info);
            model.ShowTermsAndConditionsCheckbox = await _localizationService.GetLocalizedAsync(quoteForm, x => x.ShowTermsAndConditionsCheckbox);
            model.TermsAndConditions = await _localizationService.GetLocalizedAsync(quoteForm, x => x.TermsAndConditions);
            model.SubmitButtonText = await _localizationService.GetLocalizedAsync(quoteForm, x => x.SubmitButtonText);
            model.QuoteFormAttributes = await PrepareQuoteFormAttributeModelsAsync(quoteForm, attributesXml);
            model.ShowGuestEmailField = await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest) && await _customerService.IsGuestAsync(customer);
        }

        return model;
    }


    protected virtual async Task<IList<QuoteFormAttributeModel>> PrepareQuoteFormAttributeModelsAsync(QuoteForm quoteForm, string attributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(quoteForm);

        var model = new List<QuoteFormAttributeModel>();

        var formAttributeMapping = await _formAttributeService.GetFormAttributeMappingsByQuoteFormIdAsync(quoteForm.Id);

        foreach (var attribute in formAttributeMapping)
        {
            var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(attribute.FormAttributeId);

            var attributeModel = new QuoteFormAttributeModel
            {
                Id = attribute.Id,
                QuoteFormId = quoteForm.Id,
                QuoteFormAttributeId = attribute.FormAttributeId,
                Name = await _localizationService.GetLocalizedAsync(formAttribute, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(formAttribute, x => x.Description),
                TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
                DefaultValue = await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
            };

            if (attribute.AttributeControlType == AttributeControlType.Datepicker)
            {
                if (attribute.DefaultDate.HasValue)
                {
                    attributeModel.SelectedDay = attribute.DefaultDate.Value.Day;
                    attributeModel.SelectedMonth = attribute.DefaultDate.Value.Month;
                    attributeModel.SelectedYear = attribute.DefaultDate.Value.Year;
                }

                attributeModel.ValidationMinDate = attribute.ValidationMinDate;
                attributeModel.ValidationMaxDate = attribute.ValidationMaxDate;
            }

            if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
            {
                attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            if (attribute.ShouldHaveValues())
            {
                //values
                var attributeValues = await _formAttributeService.GetFormAttributeValuesAsync(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    var valueModel = new QuoteFormAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                        ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                        IsPreSelected = attributeValue.IsPreSelected
                    };
                    attributeModel.Values.Add(valueModel);

                    //"image square" picture (with with "image squares" attribute type only)
                    if (attributeValue.ImageSquaresPictureId > 0)
                    {
                        var formAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(QuoteCartDefaults.QuoteAttributeImageSquarePictureModelKey
                            , attributeValue.ImageSquaresPictureId,
                                _webHelper.IsCurrentConnectionSecured(),
                                await _storeContext.GetCurrentStoreAsync());
                        valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(formAttributeImageSquarePictureCacheKey, async () =>
                        {
                            var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                            string fullSizeImageUrl, imageUrl;
                            (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);
                            (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                            if (imageSquaresPicture != null)
                            {
                                return new PictureModel
                                {
                                    FullSizeImageUrl = fullSizeImageUrl,
                                    ImageUrl = imageUrl
                                };
                            }

                            return new PictureModel();
                        });
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(attributesXml))
            {
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        //clear default selection                                
                        foreach (var item in attributeModel.Values)
                            item.IsPreSelected = false;

                        //select new values
                        var selectedValues = await _formAttributeParser.ParseFormAttributeValuesAsync(attributesXml);
                        foreach (var attributeValue in selectedValues)
                            foreach (var item in attributeModel.Values)
                                if (attributeValue.Id == item.Id)
                                    item.IsPreSelected = true;
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        attributeModel.DefaultValue = _formAttributeParser.ParseValues(attributesXml, attribute.Id).FirstOrDefault();
                        break;
                    case AttributeControlType.Datepicker:
                        var selectedDateStr = _formAttributeParser.ParseValues(attributesXml, attribute.Id).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(selectedDateStr) && DateTime.TryParse(selectedDateStr, out var selectedDate))
                        {
                            attributeModel.SelectedDay = selectedDate.Day;
                            attributeModel.SelectedMonth = selectedDate.Month;
                            attributeModel.SelectedYear = selectedDate.Year;
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        break;
                    case AttributeControlType.ColorSquares:
                        break;
                    case AttributeControlType.ImageSquares:
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        break;
                    default:
                        break;
                }
            }

            model.Add(attributeModel);
        }

        return model;
    }



    /// <summary>
    /// Prepare the cart item picture model
    /// </summary>
    /// <param name="qci">Quote cart item</param>
    /// <param name="pictureSize">Picture size</param>
    /// <param name="showDefaultPicture">Whether to show the default picture</param>
    /// <param name="productName">Product name</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the picture model
    /// </returns>
    public virtual async Task<PictureModel> PrepareCartItemPictureModelAsync(QuoteCartItem qci, int pictureSize, bool showDefaultPicture, string productName)
    {
        var pictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(QuoteCartDefaults.CartPictureModelKey
            , qci, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

        var model = await _staticCacheManager.GetAsync(pictureCacheKey, async () =>
        {
            var product = await _productService.GetProductByIdAsync(qci.ProductId);

            //quote cart item picture
            var qciPicture = await _pictureService.GetProductPictureAsync(product, qci.AttributesXml);

            return new PictureModel
            {
                ImageUrl = (await _pictureService.GetPictureUrlAsync(qciPicture, pictureSize, showDefaultPicture)).Url,
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
            };
        });

        return model;
    }


    #endregion
}
