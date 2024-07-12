using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paystack.Models.Request
{
    public class PaymentInitRequestModel
    {
        public PaymentInitRequestModel()
        {
            Channels = new List<string>();
        }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("invoice_limit")]
        public int InvoiceLimit { get; set; }

        [JsonProperty("metadata")]
        public string Metadata { get; set; }

        [JsonProperty("channels")]
        public IList<string> Channels { get; set; }

        [JsonProperty("split_code")]
        public string SplitCode { get; set; }

        [JsonProperty("subaccount")]
        public string SubAccount { get; set; }

        [JsonProperty("transaction_charge")]
        public int TransactionCharge { get; set; }

        [JsonProperty("bearer")]
        public string Bearer { get; set; }
    }
}
