using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record ConvertToOrderModel : BaseNopModel
{
    public ConvertToOrderModel()
    {
        AvailableAddresses = [];
        AvailablePaymentMethods = [];
        AvailableShippingMethods = [];
        AvailableShippingProviders = [];
        BillingAddress = new ();
        ShippingAddress = new ();
        PickupAddress = new ();
        QuoteRequestDetails = new ();
    }

    public QuoteRequestDetailsModel QuoteRequestDetails { get; set; }

    public CustomerModel CustomerModel { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingMethodId")]
    public string ShippingMethodId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingRateComputationMethodSystemName")]
    public string ShippingRateComputationMethodSystemName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PaymentMethodSystemName")]
    public string PaymentMethodSystemName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.BillingAddress")]
    public AddressModel BillingAddress { get; set; }

    public int BillingAddressId { get; set; }

    [NopResourceDisplayName("Admin.Orders.Fields.ShippingAddress")]
    public AddressModel ShippingAddress { get; set; }

    public int ShippingAddressId { get; set; }

    [NopResourceDisplayName("Admin.Orders.Fields.PickupAddress")]
    public AddressModel PickupAddress { get; set; }

    public int PickupAddressId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShipToSameAddress")]
    public bool ShipToSameAddress { get; set; }

    public bool SendEmailToCustomer { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PickUpInStore")]
    public bool PickUpInStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.MarkAsPaid")]
    public bool MarkAsPaid { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.OrderShippingFee")]
    public decimal OrderShippingFee { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PaymentMethodAdditionalFee")]
    public decimal PaymentMethodAdditionalFee { get; set; }

    public bool IsShippable { get; set; }

    public bool IsLoggedInAsVendor { get; set; }

    public string CustomerCurrencyCode { get; set; }

    public decimal CustomerCurrencyRate { get; set; }

    public IList<SelectListItem> AvailableAddresses { get; set; }

    public IList<SelectListItem> AvailableShippingMethods { get; set; }

    public IList<SelectListItem> AvailableShippingProviders { get; set; }

    public IList<SelectListItem> AvailablePaymentMethods { get; set; }

    public int QuoteRequestId { get; set; }
}
