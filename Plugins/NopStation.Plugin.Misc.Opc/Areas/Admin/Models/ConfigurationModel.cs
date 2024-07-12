using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Opc.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    public ConfigurationModel()
    {
        SaveBillingAddressOnChangeFields = new List<string>();
        SaveShippingAddressOnChangeFields = new List<string>();
        UpdateShippingMethodsOnChangeShippingAddressFields = new List<string>();
        UpdatePaymentMethodsOnChangeBillingAddressFields = new List<string>();
        UpdatePaymentMethodsOnChangeShippingAddressFields = new List<string>();

        AvailableAddressFields = new List<SelectListItem>();
        AvailableCountries = new List<SelectListItem>();
    }

    #region General

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.EnableOnePageCheckout")]
    public bool EnableOnePageCheckout { get; set; }
    public bool EnableOnePageCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.BypassShoppingCartPage")]
    public bool BypassShoppingCartPage { get; set; }
    public bool BypassShoppingCartPage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.ShowShoppingCartInCheckout")]
    public bool ShowShoppingCartInCheckout { get; set; }
    public bool ShowShoppingCartInCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.ShowDiscountBoxInCheckout")]
    public bool ShowDiscountBoxInCheckout { get; set; }
    public bool ShowDiscountBoxInCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.ShowGiftCardBoxInCheckout")]
    public bool ShowGiftCardBoxInCheckout { get; set; }
    public bool ShowGiftCardBoxInCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.ShowCheckoutAttributesInCheckout")]
    public bool ShowCheckoutAttributesInCheckout { get; set; }
    public bool ShowCheckoutAttributesInCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.ShowOrderReviewDataInCheckout")]
    public bool ShowOrderReviewDataInCheckout { get; set; }
    public bool ShowOrderReviewDataInCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.ShowEstimateShippingInCheckout")]
    public bool ShowEstimateShippingInCheckout { get; set; }
    public bool ShowEstimateShippingInCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.IsShoppingCartEditable")]
    public bool IsShoppingCartEditable { get; set; }
    public bool IsShoppingCartEditable_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.EnableBuyNowButton")]
    public bool EnableBuyNowButton { get; set; }
    public bool EnableBuyNowButton_OverrideForStore { get; set; }

    #endregion

    #region Billing addresses

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.PreselectPreviousBillingAddress")]
    public bool PreselectPreviousBillingAddress { get; set; }
    public bool PreselectPreviousBillingAddress_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.PreselectShipToSameAddress")]
    public bool PreselectShipToSameAddress { get; set; }
    public bool PreselectShipToSameAddress_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.DefaultBillingAddressCountryId")]
    public int DefaultBillingAddressCountryId { get; set; }
    public bool DefaultBillingAddressCountryId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.SaveBillingAddressOnChangeFields")]
    public IList<string> SaveBillingAddressOnChangeFields { get; set; }
    public bool SaveBillingAddressOnChangeFields_OverrideForStore { get; set; }

    #endregion

    #region Shipping addresses

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.PreselectPreviousShippingAddress")]
    public bool PreselectPreviousShippingAddress { get; set; }
    public bool PreselectPreviousShippingAddress_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.DefaultShippingAddressCountryId")]
    public int DefaultShippingAddressCountryId { get; set; }
    public bool DefaultShippingAddressCountryId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.SaveShippingAddressOnChangeFields")]
    public IList<string> SaveShippingAddressOnChangeFields { get; set; }
    public bool SaveShippingAddressOnChangeFields_OverrideForStore { get; set; }

    #endregion

    #region Shipping methods

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.UpdateShippingMethodsOnChangeShippingAddressFields")]
    public IList<string> UpdateShippingMethodsOnChangeShippingAddressFields { get; set; }
    public bool UpdateShippingMethodsOnChangeShippingAddressFields_OverrideForStore { get; set; }

    #endregion

    #region Payment methods

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.UpdatePaymentMethodsOnChangeBillingAddressFields")]
    public IList<string> UpdatePaymentMethodsOnChangeBillingAddressFields { get; set; }
    public bool UpdatePaymentMethodsOnChangeBillingAddressFields_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Opc.Configuration.Fields.UpdatePaymentMethodsOnChangeShippingAddressFields")]
    public IList<string> UpdatePaymentMethodsOnChangeShippingAddressFields { get; set; }
    public bool UpdatePaymentMethodsOnChangeShippingAddressFields_OverrideForStore { get; set; }

    #endregion

    public int ActiveStoreScopeConfiguration { get; set; }

    public IList<SelectListItem> AvailableAddressFields { get; set; }
    public IList<SelectListItem> AvailableCountries { get; set; }
}
