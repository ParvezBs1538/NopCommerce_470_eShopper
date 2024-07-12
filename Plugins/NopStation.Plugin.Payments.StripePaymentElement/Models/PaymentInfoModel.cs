using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.Payments.StripePaymentElement.Models;

public record PaymentInfoModel : BaseNopModel
{
    public PaymentInfoModel()
    {
        BillingAddress = new AddressModel();
        ShippingAddress = new AddressModel();
    }

    public string OrderTotal { get; set; }

    public string Theme { get; set; }

    public string Layout { get; set; }

    public string Currency { get; set; }

    public string PaymentIntentId { get; set; }

    public string PaymentIntentStatus { get; set; }

    public string Country { get; set; }

    public string PublishableKey { get; set; }

    public AddressModel BillingAddress { get; set; }

    public AddressModel ShippingAddress { get; set; }
}