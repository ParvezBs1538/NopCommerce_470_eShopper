using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class TakePaymentRequestBody
    {
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("singleUseTokenId")]
        public string SingleUseTokenId { get; set; }

        [JsonProperty("supplierBusinessCode")]
        public string SupplierBusinessCode { get; set; }

        [JsonProperty("principalAmount")]
        public double PrincipalAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("eci")]
        public string Eci { get; set; }

        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
    }
}
