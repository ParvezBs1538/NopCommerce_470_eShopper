using Nop.Core;
using Nop.Core.Caching;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class QuoteCartDefaults
{
    public const string FLYOUT_QUOTE_CART_COMPONENT_NAME = "FlyoutQuoteCart";

    public const string ADD_QUOTE_COMPONENT_NAME = "QuoteButton";

    public const string ADD_QUOTE_DETAILS_COMPONENT_NAME = "ProductDetailsQuoteButton";

    public const string MY_REQUESTS_COMPONENT_NAME = "MyQuoteRequests";

    public const string SYSTEM_NAME = "NopStation.Plugin.Misc.QuoteCart";

    public const int ACCOUNT_PANEL_TAB = 500;

    public const int ACCOUNT_REQUESTS_PAGE_SIZE = 5;

    public const string OUTPUT_PATH = "~/Plugins/" + SYSTEM_NAME;

    public static CacheKey QuoteCartItemsAllCacheKey => new("Nop.quoteCartItem.all.{0}-{1}", QuoteCartItemsByCustomerPrefix, NopEntityCacheDefaults<QuoteCartItem>.AllPrefix);

    /// <summary>
    /// Gets a key pattern for form attributes from quote form ID
    /// </summary>
    public static CacheKey FormAttributeMappingsByQuoteFormCacheKey => new("NopStation.formAttributeMapping.byQuoteForm.{0}");

    public static CacheKey FormAttributeMappingsByAttributeCacheKey => new("NopStation.formAttributeMapping.byAttribute.{0}");

    public static CacheKey FormAttributeValuesByAttributeCacheKey => new("NopStation.formAttributeValue.byAttribute.{0}");

    public static CacheKey PredefinedFormAttributeValuesByAttributeCacheKey => new("NopStation.predefinedFormAttributeValue.byAttribute.{0}");

    public static CacheKey CartPictureModelKey => new("Nop.quoteCartItem.picture-{0}-{1}-{2}-{3}-{4}-{5}", "Nop.quoteCartItem.picture");

    public static CacheKey PictureModelKey => new("Nop.quoteCartPicture.picture-{0}-{1}-{2}", "Nop.quoteCartItem.picture");

    public static CacheKey QuoteAttributeImageSquarePictureModelKey => new("Nop.quoteCartPicture.formAttribute.imageSquare.picture-{0}-{1}-{2}", QuoteAttributeImageSquarePicturePrefixCacheKey);
    public static string QuoteAttributeImageSquarePicturePrefixCacheKey => "Nop.quoteCartPicture.imageSquare.picture";

    public static string QuoteCartItemsByCustomerPrefix => "Nop.quoteCartItem.all.{0}";
    public static string QuoteCartCartUnder => "quote_cart_under";

    public static string QuoteCartAttachmentsPath => "quoteCartAttachment/";
    public static string RequestsExportedFileName => "AllQuoteRequests.xlsx";
    public static string RequestsExportedFileType => MimeTypes.TextXlsx;

    public static string FormFieldPrefix => "formfield_";

    //notification templates 
    public const string QUOTE_CUSTOMER_REQUEST_SUBMITTED_NOTIFICATION = SYSTEM_NAME + ".QuoteCustomer.RequestSubmitted";
    public const string QUOTE_STORE_REQUEST_SUBMITTED_NOTIFICATION = SYSTEM_NAME + ".QuoteStore.RequestSubmitted";
    public const string REQUEST_NEW_REPLY_CUSTOMER_NOTIFICATION = SYSTEM_NAME + ".QuoteCustomer.ReplySent";
    public const string REQUEST_NEW_REPLY_STORE_NOTIFICATION = SYSTEM_NAME + ".QuoteStore.ReplySent";
    public const string QUOTE_CUSTOMER_QUOTE_OFFER = SYSTEM_NAME + ".QuoteCustomer.QuoteOffer";
    public const string QUOTE_CUSTOMER_QUOTE_OFFER_TABLE = SYSTEM_NAME + ".QuoteCustomer.QuoteOffer.Table";
    public const string QUOTE_CUSTOMER_QUOTE_OFFER_TABLE_ROW = SYSTEM_NAME + ".QuoteCustomer.QuoteOffer.TableRow";
    public const string REQUEST_CUSTOMER_STATUS_CHANGED_NOTIFICATION = SYSTEM_NAME + ".QuoteCustomer.StatusChanged";
}