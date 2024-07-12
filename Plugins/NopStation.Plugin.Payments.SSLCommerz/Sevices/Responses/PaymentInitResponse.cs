using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Responses
{
    public class PaymentInitResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("failedreason")]
        public string FailedReason { get; set; }

        [JsonProperty("sessionkey")]
        public string SessionKey { get; set; }

        [JsonProperty("gw")]
        public GatewayResponse Gateway { get; set; }

        [JsonProperty("redirectGatewayURL")]
        public string RedirectGatewayURL { get; set; }

        [JsonProperty("directPaymentURLBank")]
        public string DirectPaymentURLBank { get; set; }

        [JsonProperty("directPaymentURLCard")]
        public string DirectPaymentURLCard { get; set; }

        [JsonProperty("directPaymentURL")]
        public string DirectPaymentURL { get; set; }

        [JsonProperty("redirectGatewayURLFailed")]
        public string RedirectGatewayURLFailed { get; set; }

        [JsonProperty("GatewayPageURL")]
        public string GatewayPageURL { get; set; }

        [JsonProperty("storeBanner")]
        public string StoreBanner { get; set; }

        [JsonProperty("storeLogo")]
        public string StoreLogo { get; set; }

        [JsonProperty("store_name")]
        public string StoreName { get; set; }

        [JsonProperty("desc")]
        public List<DescResponse> Desc { get; set; }

        [JsonProperty("is_direct_pay_enable")]
        public string IsDirectPayEnable { get; set; }


        public class GatewayResponse
        {
            [JsonProperty("visa")]
            public string Visa { get; set; }

            [JsonProperty("master")]
            public string Master { get; set; }

            [JsonProperty("amex")]
            public string Amex { get; set; }

            [JsonProperty("othercards")]
            public string Othercards { get; set; }

            [JsonProperty("internetbanking")]
            public string Internetbanking { get; set; }

            [JsonProperty("mobilebanking")]
            public string Mobilebanking { get; set; }
        }

        public class DescResponse
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("logo")]
            public string Logo { get; set; }

            [JsonProperty("gw")]
            public string Gw { get; set; }

            [JsonProperty("r_flag")]
            public string RFlag { get; set; }

            [JsonProperty("redirectGatewayURL")]
            public string RedirectGatewayURL { get; set; }
        }
    }
}
