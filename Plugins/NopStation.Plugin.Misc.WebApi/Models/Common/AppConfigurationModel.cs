using System.Collections.Generic;
using Nop.Web.Models.Common;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.WebApi.Models.Common
{
    public class AppConfigurationModel
    {
        public AppConfigurationModel()
        {
            StringResources = new List<KeyValueApi>();
            CurrencyNavSelector = new CurrencySelectorModel();
            LanguageNavSelector = new LanguageSelectorModel();
        }

        public bool ShowHomepageSlider { get; set; }
        public bool ShowFeaturedProducts { get; set; }
        public bool SliderAutoPlay { get; set; }
        public int SliderAutoPlayTimeout { get; set; }
        public bool ShowBestsellersOnHomepage { get; set; }
        public bool ShowHomepageCategoryProducts { get; set; }
        public bool ShowManufacturers { get; set; }
        public bool Rtl { get; set; }
        public string AndroidVersion { get; set; }
        public bool AndriodForceUpdate { get; set; }
        public string PlayStoreUrl { get; set; }
        public string IOSVersion { get; set; }
        public bool IOSForceUpdate { get; set; }
        public string AppStoreUrl { get; set; }
        public string LogoUrl { get; set; }
        public int TotalShoppingCartProducts { get; set; }
        public int TotalWishListProducts { get; set; }
        public bool ShowAllVendors { get; set; }
        public bool AnonymousCheckoutAllowed { get; set; }
        public bool ShowChangeBaseUrlPanel { get; set; }
        public bool HasReturnRequests { get; set; }
        public bool HideDownloadableProducts { get; set; }
        public string PrimaryThemeColor { get; set; }
        public string TopBarBackgroundColor { get; set; }
        public string TopBarTextColor { get; set; }
        public string BottomBarBackgroundColor { get; set; }
        public string BottomBarActiveColor { get; set; }
        public string BottomBarInactiveColor { get; set; }
        public string GradientStartingColor { get; set; }
        public string GradientMiddleColor { get; set; }
        public string GradientEndingColor { get; set; }
        public bool GradientEnabled { get; set; }
        public int IOSProductPriceTextSize { get; set; }
        public int AndroidProductPriceTextSize { get; set; }
        public int IonicProductPriceTextSize { get; set; }
        public bool NewProductsEnabled { get; set; }
        public bool RecentlyViewedProductsEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }
        public bool AllowCustomersToUploadAvatars { get; set; }
        public int AvatarMaximumSizeBytes { get; set; }
        public bool HideBackInStockSubscriptionsTab { get; set; }
        public bool StoreClosed { get; set; }
        public bool GdprEnabled { get; set; }
        public bool AllowCustomersToDeleteAccount { get; set; }
        public bool WishlistEnabled { get; set; }
        public bool ShoppingCartEnabled { get; set; }

        public CurrencySelectorModel CurrencyNavSelector { get; set; }
        public LanguageSelectorModel LanguageNavSelector { get; set; }
        public IList<KeyValueApi> StringResources { get; set; }
    }
}