using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.AfterpayPayment.Models
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
        public Amount OriginalAmount { get; set; }

        [JsonProperty("openToCaptureAmount")]
        public Amount OpenToCaptureAmount { get; set; }

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
