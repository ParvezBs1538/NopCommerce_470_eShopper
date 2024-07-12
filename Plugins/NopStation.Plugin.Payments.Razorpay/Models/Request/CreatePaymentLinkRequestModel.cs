using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Razorpay.Models.Request
{
    public class CreatePaymentLinkRequestModel
    {
        public CreatePaymentLinkRequestModel()
        {
            Customer = new CustomerModel();
            Notify = new NotifyModel();
            Notes = new Dictionary<string, object>();
        }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("accept_partial")]
        public bool AcceptPartial { get; set; }

        [JsonProperty("first_min_partial_amount")]
        public int FirstMinPartialAmount { get; set; }

        [JsonProperty("expire_by")]
        public long ExpireBy { get; set; }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("customer")]
        public CustomerModel Customer { get; set; }

        [JsonProperty("notify")]
        public NotifyModel Notify { get; set; }

        [JsonProperty("reminder_enable")]
        public bool ReminderEnable { get; set; }

        [JsonProperty("notes")]
        public IDictionary<string, object> Notes { get; set; }

        [JsonProperty("callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty("callback_method")]
        public string CallbackMethod { get; set; }
    }
}
