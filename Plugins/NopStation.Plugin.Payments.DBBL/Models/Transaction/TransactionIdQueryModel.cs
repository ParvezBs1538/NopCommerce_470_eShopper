using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.DBBL.Models.Transaction
{
    public class TransactionIdQueryModel
    {
        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "pwd")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "submername")]
        public string SubMerchantName { get; set; }

        [JsonProperty(PropertyName = "submerid")]
        public string SubMerchantId { get; set; }

        [JsonProperty(PropertyName = "terminalid")]
        public string TerminalId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "cardtype")]
        public string CardType { get; set; }

        [JsonProperty(PropertyName = "txnrefnum")]
        public string ReferenceNumber { get; set; }

        [JsonProperty(PropertyName = "clientip")]
        public string ClientIp { get; set; }
    }
}
