namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class RouteNames
{
    //home page
    public static string Homepage => "Homepage";

    //login
    public static string Login => "Login";

    // multi-factor verification digit code page
    public static string MultiFactorVerification => "MultiFactorVerification";

    //register
    public static string Register => "Register";

    //logout
    public static string Logout => "Logout";

    //shopping cart
    public static string ShoppingCart => "ShoppingCart";

    //estimate shipping (AJAX)
    public static string EstimateShipping => "EstimateShipping";

    //wishlist
    public static string Wishlist => "Wishlist";

    //customer account links
    public static string CustomerInfo => "CustomerInfo";

    public static string CustomerAddresses => "CustomerAddresses";

    public static string CustomerOrders => "CustomerOrders";

    //contact us
    public static string ContactUs => "ContactUs";

    //product search
    public static string ProductSearch => "ProductSearch";

    //autocomplete search term (AJAX)
    public static string ProductSearchAutoComplete => "ProductSearchAutoComplete";

    //change currency
    public static string ChangeCurrency => "ChangeCurrency";

    //change language
    public static string ChangeLanguage => "ChangeLanguage";

    //change tax
    public static string ChangeTaxType => "ChangeTaxType";

    //recently viewed products
    public static string RecentlyViewedProducts => "RecentlyViewedProducts";

    //new products
    public static string NewProducts => "NewProducts";

    //blog
    public static string Blog => "Blog";

    //news
    public static string NewsArchive => "NewsArchive";

    //forum
    public static string Boards => "Boards";

    //compare products
    public static string CompareProducts => "CompareProducts";

    //product tags
    public static string ProductTagsAll => "ProductTagsAll";

    //manufacturers
    public static string ManufacturerList => "ManufacturerList";

    //vendors
    public static string VendorList => "VendorList";

    //add product to cart (without any attributes and options). used on catalog pages. (AJAX)
    public static string AddProductToCartCatalog => "AddProductToCart-Catalog";

    //add product to cart (with attributes and options). used on the product details pages. (AJAX)
    public static string AddProductToCartDetails => "AddProductToCart-Details";

    //comparing products (AJAX)
    public static string AddProductToCompare => "AddProductToCompare";

    //product email a friend
    public static string ProductEmailAFriend => "ProductEmailAFriend";

    //reviews
    public static string ProductReviews => "ProductReviews";

    public static string CustomerProductReviews => "CustomerProductReviews";

    public static string CustomerProductReviewsPaged => "CustomerProductReviewsPaged";

    //back in stock notifications (AJAX)
    public static string BackInStockSubscribePopup => "BackInStockSubscribePopup";

    public static string BackInStockSubscribeSend => "BackInStockSubscribeSend";

    //downloads (file result)
    public static string GetSampleDownload => "GetSampleDownload";

    //checkout pages
    public static string Checkout => "Checkout";

    public static string CheckoutOnePage => "CheckoutOnePage";

    public static string CheckoutShippingAddress => "CheckoutShippingAddress";

    public static string CheckoutSelectShippingAddress => "CheckoutSelectShippingAddress";

    public static string CheckoutBillingAddress => "CheckoutBillingAddress";

    public static string CheckoutSelectBillingAddress => "CheckoutSelectBillingAddress";

    public static string CheckoutShippingMethod => "CheckoutShippingMethod";

    public static string CheckoutPaymentMethod => "CheckoutPaymentMethod";

    public static string CheckoutPaymentInfo => "CheckoutPaymentInfo";

    public static string CheckoutConfirm => "CheckoutConfirm";

    public static string CheckoutCompleted => "CheckoutCompleted";

    //subscribe newsletters (AJAX)
    public static string SubscribeNewsletter => "SubscribeNewsletter";

    //email wishlist
    public static string EmailWishlist => "EmailWishlist";

    //login page for checkout as guest
    public static string LoginCheckoutAsGuest => "LoginCheckoutAsGuest";

    //register result page
    public static string RegisterResult => "RegisterResult";

    //check username availability (AJAX)
    public static string CheckUsernameAvailability => "CheckUsernameAvailability";

    //passwordrecovery
    public static string PasswordRecovery => "PasswordRecovery";

    //password recovery confirmation
    public static string PasswordRecoveryConfirm => "PasswordRecoveryConfirm";

    //topics (AJAX)
    public static string TopicPopup => "TopicPopup";

    //blog
    public static string BlogByTag => "BlogByTag";

    public static string BlogByMonth => "BlogByMonth";

    //blog RSS (file result)
    public static string BlogRSS => "BlogRSS";

    //news RSS (file result)
    public static string NewsRSS => "NewsRSS";

    //set review helpfulness (AJAX)
    public static string SetProductReviewHelpfulness => "SetProductReviewHelpfulness";

    //customer account links
    public static string CustomerReturnRequests => "CustomerReturnRequests";

    public static string CustomerDownloadableProducts => "CustomerDownloadableProducts";

    public static string CustomerBackInStockSubscriptions => "CustomerBackInStockSubscriptions";

    public static string CustomerRewardPoints => "CustomerRewardPoints";

    public static string CustomerRewardPointsPaged => "CustomerRewardPointsPaged";

    public static string CustomerChangePassword => "CustomerChangePassword";

    public static string CustomerAvatar => "CustomerAvatar";

    public static string AccountActivation => "AccountActivation";

    public static string EmailRevalidation => "EmailRevalidation";

    public static string CustomerForumSubscriptions => "CustomerForumSubscriptions";

    public static string CustomerAddressEdit => "CustomerAddressEdit";

    public static string CustomerAddressAdd => "CustomerAddressAdd";

    public static string CustomerMultiFactorAuthenticationProviderConfig => "CustomerMultiFactorAuthenticationProviderConfig";

    //customer profile page
    public static string CustomerProfile => "CustomerProfile";

    public static string CustomerProfilePaged => "CustomerProfilePaged";

    //orders
    public static string OrderDetails => "OrderDetails";

    public static string ShipmentDetails => "ShipmentDetails";

    public static string ReturnRequest => "ReturnRequest";

    public static string ReOrder => "ReOrder";

    //pdf invoice (file result)
    public static string GetOrderPdfInvoice => "GetOrderPdfInvoice";

    public static string PrintOrderDetails => "PrintOrderDetails";

    //order downloads (file result)
    public static string GetDownload => "GetDownload";

    public static string GetLicense => "GetLicense";

    public static string DownloadUserAgreement => "DownloadUserAgreement";

    public static string GetOrderNoteFile => "GetOrderNoteFile";

    //contact vendor
    public static string ContactVendor => "ContactVendor";

    //apply for vendor account
    public static string ApplyVendorAccount => "ApplyVendorAccount";

    //vendor info
    public static string CustomerVendorInfo => "CustomerVendorInfo";

    //customer GDPR
    public static string GdprTools => "GdprTools";

    //customer check gift card balance 
    public static string CheckGiftCardBalance => "CheckGiftCardBalance";

    //customer multi-factor authentication settings 
    public static string MultiFactorAuthenticationSettings => "MultiFactorAuthenticationSettings";

    //poll vote (AJAX)
    public static string PollVote => "PollVote";

    //comparing products
    public static string RemoveProductFromCompareList => "RemoveProductFromCompareList";

    public static string ClearCompareList => "ClearCompareList";

    //new RSS (file result)
    public static string NewProductsRSS => "NewProductsRSS";

    //get state list by country ID (AJAX)
    public static string GetStatesByCountryId => "GetStatesByCountryId";

    //EU Cookie law accept button handler (AJAX)
    public static string EuCookieLawAccept => "EuCookieLawAccept";

    //authenticate topic (AJAX)
    public static string TopicAuthenticate => "TopicAuthenticate";

    //prepare top menu (AJAX)
    public static string GetCatalogRoot => "GetCatalogRoot";

    public static string GetCatalogSubCategories => "GetCatalogSubCategories";

    //Catalog products (AJAX)
    public static string GetCategoryProducts => "GetCategoryProducts";

    public static string GetManufacturerProducts => "GetManufacturerProducts";

    public static string GetTagProducts => "GetTagProducts";

    public static string SearchProducts => "SearchProducts";

    public static string GetVendorProducts => "GetVendorProducts";

    //product combinations (AJAX)
    public static string GetProductCombinations => "GetProductCombinations";

    //product attributes with "upload file" type (AJAX)
    public static string UploadFileProductAttribute => "UploadFileProductAttribute";

    //checkout attributes with "upload file" type (AJAX)
    public static string UploadFileCheckoutAttribute => "UploadFileCheckoutAttribute";

    //return request with "upload file" support (AJAX)
    public static string UploadFileReturnRequest => "UploadFileReturnRequest";

    //forums
    public static string ActiveDiscussions => "ActiveDiscussions";

    public static string ActiveDiscussionsPaged => "ActiveDiscussionsPaged";

    //forums RSS (file result)
    public static string ActiveDiscussionsRSS => "ActiveDiscussionsRSS";

    public static string PostEdit => "PostEdit";

    public static string PostDelete => "PostDelete";

    public static string PostCreate => "PostCreate";

    public static string PostCreateQuote => "PostCreateQuote";

    public static string TopicEdit => "TopicEdit";

    public static string TopicDelete => "TopicDelete";

    public static string TopicCreate => "TopicCreate";

    public static string TopicMove => "TopicMove";

    //topic watch (AJAX)
    public static string TopicWatch => "TopicWatch";

    public static string TopicSlug => "TopicSlug";

    public static string TopicSlugPaged => "TopicSlugPaged";

    //forum watch (AJAX)
    public static string ForumWatch => "ForumWatch";

    //forums RSS (file result)
    public static string ForumRSS => "ForumRSS";

    public static string ForumSlug => "ForumSlug";

    public static string ForumSlugPaged => "ForumSlugPaged";

    public static string ForumGroupSlug => "ForumGroupSlug";

    public static string Search => "Search";

    //private messages
    public static string PrivateMessages => "PrivateMessages";

    public static string PrivateMessagesPaged => "PrivateMessagesPaged";

    public static string PrivateMessagesInbox => "PrivateMessagesInbox";

    public static string PrivateMessagesSent => "PrivateMessagesSent";

    public static string SendPM => "SendPM";

    public static string SendPMReply => "SendPMReply";

    public static string ViewPM => "ViewPM";

    public static string DeletePM => "DeletePM";

    //activate newsletters
    public static string NewsletterActivation => "NewsletterActivation";

    //robots.txt (file result)
    public static string RobotsTxt => "robots.txt";

    //sitemap
    public static string Sitemap => "Sitemap";

    //sitemap.xml (file result)
    public static string SitemapXML => "sitemap.xml";

    public static string SitemapIndexedXML => "sitemap-indexed.xml";

    //store closed
    public static string StoreClosed => "StoreClosed";

    //install
    public static string Installation => "Installation";

    //error page
    public static string Error => "Error";

    //page not found
    public static string PageNotFound => "PageNotFound";
}