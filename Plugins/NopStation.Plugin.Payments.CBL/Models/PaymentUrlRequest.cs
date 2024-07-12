using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.CBL.Models
{
    public class PaymentUrlRequest
    {
        [JsonProperty("userName")]
        public string MerchanUserName { get; set; }
        [JsonProperty("password")]
        public string MerchanPassword { get; set; }
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("amount")]
        public double PaymentAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("returnUrl")]
        public string ReturnUrl { get; set; }

        [JsonProperty("addParams")]
        public string AddParams { get; set; }

        [JsonProperty("transactionId")]
        public string OrderId { get; set; }

        [JsonProperty("phoneNo")]
        public string PhoneNo { get; set; }
    }
}
