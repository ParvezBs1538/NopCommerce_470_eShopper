using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.CBL.Models
{
    public class OrderResponse
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("cardHoldername")]
        public string CardHoldername { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("pan")]
        public string Pan { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("orderStatus")]
        public string OrderStatus { get; set; }

        [JsonProperty("declineReason")]
        public object DeclineReason { get; set; }

        [JsonProperty("approvalCode")]
        public string ApprovalCode { get; set; }

        [JsonProperty("merchantTranID")]
        public string MerchantTranID { get; set; }

        [JsonProperty("rrn")]
        public string Rrn { get; set; }

        [JsonProperty("orderCreatedatetime")]
        public string OrderCreatedatetime { get; set; }

        [JsonProperty("approveDatetime")]
        public string ApproveDatetime { get; set; }

        [JsonProperty("twolID")]
        public string TwolID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("addParam")]
        public string AddParam { get; set; }

        [JsonProperty("sessionID")]
        public string SessionID { get; set; }
    }
}
