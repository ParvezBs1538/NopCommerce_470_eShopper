using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            LocaleResourceSearchModel = new LocaleResourceSearchModel();
            AvailableLanguages = new List<SelectListItem>();
            SelectedResourceIds = new List<int>();
            AvailableBarcodeScanKeys = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowHomepageSlider")]
        public bool ShowHomepageSlider { get; set; }
        public bool ShowHomepageSlider_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.MaximumNumberOfHomePageSliders")]
        public int MaximumNumberOfHomePageSliders { get; set; }
        public bool MaximumNumberOfHomePageSliders_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.SliderAutoPlay")]
        public bool SliderAutoPlay { get; set; }
        public bool SliderAutoPlay_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.SliderAutoPlayTimeout")]
        public int SliderAutoPlayTimeout { get; set; }
        public bool SliderAutoPlayTimeout_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowFeaturedProducts")]
        public bool ShowFeaturedProducts { get; set; }
        public bool ShowFeaturedProducts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowBestsellersOnHomepage")]
        public bool ShowBestsellersOnHomepage { get; set; }
        public bool ShowBestsellersOnHomepage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.NumberOfBestsellersOnHomepage")]
        public int NumberOfBestsellersOnHomepage { get; set; }
        public bool NumberOfBestsellersOnHomepage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowHomepageCategoryProducts")]
        public bool ShowHomepageCategoryProducts { get; set; }
        public bool ShowHomepageCategoryProducts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowSubCategoryProducts")]
        public bool ShowSubCategoryProducts { get; set; }
        public bool ShowSubCategoryProducts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.NumberOfHomepageCategoryProducts")]
        public int NumberOfHomepageCategoryProducts { get; set; }
        public bool NumberOfHomepageCategoryProducts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowManufacturers")]
        public bool ShowManufacturers { get; set; }
        public bool ShowManufacturers_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.NumberOfManufacturers")]
        public int NumberOfManufacturers { get; set; }
        public bool NumberOfManufacturers_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.IOSProductPriceTextSize")]
        public int IOSProductPriceTextSize { get; set; }
        public bool IOSProductPriceTextSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.IonicProductPriceTextSize")]
        public int IonicProductPriceTextSize { get; set; }
        public bool IonicProductPriceTextSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.AndroidProductPriceTextSize")]
        public int AndroidProductPriceTextSize { get; set; }
        public bool AndroidProductPriceTextSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.EnableJwtSecurity")]
        public bool EnableJwtSecurity { get; set; }
        public bool EnableJwtSecurity_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.TokenKey")]
        public string TokenKey { get; set; }
        public bool TokenKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.TokenSecret")]
        public string TokenSecret { get; set; }
        public bool TokenSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.CheckIat")]
        public bool CheckIat { get; set; }
        public bool CheckIat_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.TokenSecondsValid")]
        public int TokenSecondsValid { get; set; }
        public bool TokenSecondsValid_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.AndroidVersion")]
        public string AndroidVersion { get; set; }
        public bool AndroidVersion_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.AndriodForceUpdate")]
        public bool AndriodForceUpdate { get; set; }
        public bool AndriodForceUpdate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.PlayStoreUrl")]
        public string PlayStoreUrl { get; set; }
        public bool PlayStoreUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.IOSVersion")]
        public string IOSVersion { get; set; }
        public bool IOSVersion_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.IOSForceUpdate")]
        public bool IOSForceUpdate { get; set; }
        public bool IOSForceUpdate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.AppStoreUrl")]
        public string AppStoreUrl { get; set; }
        public bool AppStoreUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.LogoId")]
        [UIHint("Picture")]
        public int LogoId { get; set; }
        public bool LogoId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.LogoSize")]
        public int LogoSize { get; set; }
        public bool LogoSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ShowChangeBaseUrlPanel")]
        public bool ShowChangeBaseUrlPanel { get; set; }
        public bool ShowChangeBaseUrlPanel_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.PrimaryThemeColor")]
        public string PrimaryThemeColor { get; set; }
        public bool PrimaryThemeColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.TopBarBackgroundColor")]
        public string TopBarBackgroundColor { get; set; }
        public bool TopBarBackgroundColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.TopBarTextColor")]
        public string TopBarTextColor { get; set; }
        public bool TopBarTextColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.BottomBarBackgroundColor")]
        public string BottomBarBackgroundColor { get; set; }
        public bool BottomBarBackgroundColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.BottomBarActiveColor")]
        public string BottomBarActiveColor { get; set; }
        public bool BottomBarActiveColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.BottomBarInactiveColor")]
        public string BottomBarInactiveColor { get; set; }
        public bool BottomBarInactiveColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ResetToDefaultColors")]
        public bool ResetToDefaultColors { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.GradientStartingColor")]
        public string GradientStartingColor { get; set; }
        public bool GradientStartingColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.GradientMiddleColor")]
        public string GradientMiddleColor { get; set; }
        public bool GradientMiddleColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.GradientEndingColor")]
        public string GradientEndingColor { get; set; }
        public bool GradientEndingColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.GradientEnabled")]
        public bool GradientEnabled { get; set; }
        public bool GradientEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.ProductBarcodeScanKey")]
        public int ProductBarcodeScanKeyId { get; set; }
        public bool ProductBarcodeScanKeyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.AllowCustomersToDeleteAccount")]
        public bool AllowCustomersToDeleteAccount { get; set; }
        public bool AllowCustomersToDeleteAccount_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Configuration.Fields.SearchLanguageId")]
        public int SearchLanguageId { get; set; }

        public LocaleResourceSearchModel LocaleResourceSearchModel { get; set; }

        public IList<SelectListItem> AvailableLanguages { get; set; }

        public IList<int> SelectedResourceIds { get; set; }

        public IList<SelectListItem> AvailableBarcodeScanKeys { get; set; }
    }
}