using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paystack.Models.Response
{
    public class PaymentInitResponseModel
    {
        public PaymentInitResponseModel()
        {
            Data = new InitDataModel();
        }

        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public InitDataModel Data { get; set; }


        public class InitDataModel
        {
            [JsonProperty("authorization_url")]
            public string AuthorizationUrl { get; set; }

            [JsonProperty("access_code")]
            public string AccessCode { get; set; }

            [JsonProperty("reference")]
            public string Reference { get; set; }
        }
    }
}
