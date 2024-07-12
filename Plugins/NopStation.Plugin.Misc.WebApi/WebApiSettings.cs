using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.WebApi
{
    public class WebApiSettings : ISettings
    {
        public bool EnableJwtSecurity { get; set; }

        public bool ShowHomepageSlider { get; set; }

        public int MaximumNumberOfHomePageSliders { get; set; }

        public bool SliderAutoPlay { get; set; }

        public int SliderAutoPlayTimeout { get; set; }

        public bool ShowFeaturedProducts { get; set; }

        public bool ShowBestsellersOnHomepage { get; set; }

        public int NumberOfBestsellersOnHomepage { get; set; }

        public bool ShowHomepageCategoryProducts { get; set; }

        public bool ShowSubCategoryProducts { get; set; }

        public int NumberOfHomepageCategoryProducts { get; set; }

        public bool ShowManufacturers { get; set; }

        public int NumberOfManufacturers { get; set; }

        public int AndroidProductPriceTextSize { get; set; }

        public int IOSProductPriceTextSize { get; set; }

        public int IonicProductPriceTextSize { get; set; }

        public string SecretKey { get; set; }

        public string TokenKey { get; set; }

        public string TokenSecret { get; set; }

        public bool CheckIat { get; set; }

        public int TokenSecondsValid { get; set; }

        public string AndroidVersion { get; set; }

        public bool AndriodForceUpdate { get; set; }

        public string PlayStoreUrl { get; set; }

        public string IOSVersion { get; set; }

        public bool IOSForceUpdate { get; set; }

        public string AppStoreUrl { get; set; }

        public int LogoId { get; set; }

        public int LogoSize { get; set; }

        public bool ShowChangeBaseUrlPanel { get; set; }

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

        public int ProductBarcodeScanKeyId { get; set; }

        public bool AllowCustomersToDeleteAccount { get; set; }
    }
}
