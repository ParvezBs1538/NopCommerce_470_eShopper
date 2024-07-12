using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Afterpay.Models
{
    public class AuthResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("originalAmount")]
        public PaymentAfterpayModel OriginalAmount { get; set; }

        [JsonProperty("openToCaptureAmount")]
        public PaymentAfterpayModel OpenToCaptureAmount { get; set; }

        [JsonProperty("paymentState")]
        public string PaymentState { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("refunds")]
        public List<object> Refunds { get; set; }

        [JsonProperty("orderDetails")]
        public PaymentUrlRequest OrderDetails { get; set; }

        [JsonProperty("events")]
        public List<Event> Events { get; set; }
    }
}
