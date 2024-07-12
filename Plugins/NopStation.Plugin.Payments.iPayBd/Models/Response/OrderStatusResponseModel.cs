using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.iPayBd.Models.Response
{
    public class OrderStatusResponseModel
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("transactionTime")]
        public string TransactionTime { get; set; }
    }
}
