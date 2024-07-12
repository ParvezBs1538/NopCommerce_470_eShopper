using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.QuickView.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowRelatedProducts")]
    public bool ShowRelatedProducts { get; set; }
    public bool ShowRelatedProducts_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowAlsoPurchasedProducts")]
    public bool ShowAlsoPurchasedProducts { get; set; }
    public bool ShowAlsoPurchasedProducts_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.EnablePictureZoom")]
    public bool EnablePictureZoom { get; set; }
    public bool EnablePictureZoom_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowShortDescription")]
    public bool ShowShortDescription { get; set; }
    public bool ShowShortDescription_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowFullDescription")]
    public bool ShowFullDescription { get; set; }
    public bool ShowFullDescription_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowAddToWishlistButton")]
    public bool ShowAddToWishlistButton { get; set; }
    public bool ShowAddToWishlistButton_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowCompareProductsButton")]
    public bool ShowCompareProductsButton { get; set; }
    public bool ShowCompareProductsButton_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowProductEmailAFriendButton")]
    public bool ShowProductEmailAFriendButton { get; set; }
    public bool ShowProductEmailAFriendButton_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowProductReviewOverview")]
    public bool ShowProductReviewOverview { get; set; }
    public bool ShowProductReviewOverview_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowProductManufacturers")]
    public bool ShowProductManufacturers { get; set; }
    public bool ShowProductManufacturers_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowAvailability")]
    public bool ShowAvailability { get; set; }
    public bool ShowAvailability_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowDeliveryInfo")]
    public bool ShowDeliveryInfo { get; set; }
    public bool ShowDeliveryInfo_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowProductSpecifications")]
    public bool ShowProductSpecifications { get; set; }
    public bool ShowProductSpecifications_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.QuickView.Configuration.Fields.ShowProductTags")]
    public bool ShowProductTags { get; set; }
    public bool ShowProductTags_OverrideForStore { get; set; }

    public bool PictureZoomPluginInstalled { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}