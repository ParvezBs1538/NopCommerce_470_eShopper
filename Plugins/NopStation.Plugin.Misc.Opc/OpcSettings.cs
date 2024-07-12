using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.Opc;

public class OpcSettings : ISettings
{
    public OpcSettings()
    {
        SaveBillingAddressOnChangeFields = new List<string>();
        SaveShippingAddressOnChangeFields = new List<string>();
        UpdateShippingMethodsOnChangeShippingAddressFields = new List<string>();
        UpdatePaymentMethodsOnChangeBillingAddressFields = new List<string>();
        UpdatePaymentMethodsOnChangeShippingAddressFields = new List<string>();
    }

    #region General

    public bool EnableOnePageCheckout { get; set; }

    public bool BypassShoppingCartPage { get; set; }

    public bool ShowShoppingCartInCheckout { get; set; }

    public bool ShowDiscountBoxInCheckout { get; set; }

    public bool ShowGiftCardBoxInCheckout { get; set; }

    public bool ShowCheckoutAttributesInCheckout { get; set; }

    public bool ShowOrderReviewDataInCheckout { get; set; }

    public bool ShowEstimateShippingInCheckout { get; set; }

    public bool IsShoppingCartEditable { get; set; }

    public bool EnableBuyNowButton { get; set; }

    #endregion

    #region Billing addresses

    public bool PreselectPreviousBillingAddress { get; set; }

    public bool PreselectShipToSameAddress { get; set; }

    public int DefaultBillingAddressCountryId { get; set; }

    public List<string> SaveBillingAddressOnChangeFields { get; set; }

    #endregion

    #region Shipping addresses

    public bool PreselectPreviousShippingAddress { get; set; }

    public int DefaultShippingAddressCountryId { get; set; }

    public List<string> SaveShippingAddressOnChangeFields { get; set; }

    #endregion

    #region Shipping methods

    public List<string> UpdateShippingMethodsOnChangeShippingAddressFields { get; set; }

    #endregion

    #region Payment methods

    public List<string> UpdatePaymentMethodsOnChangeBillingAddressFields { get; set; }

    public List<string> UpdatePaymentMethodsOnChangeShippingAddressFields { get; set; }

    #endregion
}