using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.StripePaymentElement.Models;

public class CreatePaymentIntentRequest
{
    [JsonProperty("amount")]
    public long? Amount { get; set; }

    [JsonProperty("paymentMethodType")]
    public string PaymentMethodType { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }
}