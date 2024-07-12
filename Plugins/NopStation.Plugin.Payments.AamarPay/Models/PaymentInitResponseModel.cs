
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.AamarPay.Models;

public class PaymentInitResponseModel
{
    [JsonProperty("result")]
    public bool Result { get; set; }

    [JsonProperty("payment_url")]
    public string PaymentUrl { get; set; }
}
