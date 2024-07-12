using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Razorpay.Models.Response
{
    public class RefundResponseModel : BaseErrorResponseModel
    {
        public RefundResponseModel()
        {
            AcquirerData = new AcquirerDataModel();
            Error = new ErrorModel();
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty("receipt")]
        public string Receipt { get; set; }

        [JsonProperty("acquirer_data")]
        public AcquirerDataModel AcquirerData { get; set; }

        [JsonProperty("created_at")]
        public int CreatedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("speed_processed")]
        public string SpeedProcessed { get; set; }

        [JsonProperty("speed_requested")]
        public string SpeedRequested { get; set; }

        public class AcquirerDataModel
        {
            [JsonProperty("arn")]
            public object Arn { get; set; }
        }
    }
}
