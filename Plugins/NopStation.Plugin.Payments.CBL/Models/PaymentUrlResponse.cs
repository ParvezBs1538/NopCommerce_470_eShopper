using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.CBL.Models
{
    public class PaymentUrlResponse
    {
        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty("transactionId")]
        public string GatewayTransactionId { get; set; }

        public string TransactionId { get; set; }

        [JsonProperty("retuenUrl")]
        public string RetuenUrl { get; set; }

    }
}
