using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.iPayBd.Models.Response
{
    public class CreateOrderResponseModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("paymentUrl")]
        public string PaymentUrl { get; set; }

        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }
    }
}
