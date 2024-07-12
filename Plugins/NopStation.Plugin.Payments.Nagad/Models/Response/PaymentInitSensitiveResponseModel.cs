using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models.Response
{
    public class PaymentInitSensitiveResponseModel
    {
        [JsonProperty("paymentReferenceId")]
        public string PaymentReferenceId { get; set; }
        [JsonProperty("acceptDateTime")]
        public string AcceptDateTime { get; set; }
        [JsonProperty("challenge")]
        public string Challenge { get; set; }
    }
}
