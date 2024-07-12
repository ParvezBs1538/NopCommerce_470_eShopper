using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Razorpay.Models.Response
{
    public class CreatePaymentLinkResponseModel : BaseErrorResponseModel
    {
        public CreatePaymentLinkResponseModel()
        {
            Customer = new CustomerModel();
            Notify = new NotifyModel();
            Notes = new Dictionary<string, object>();
            Error = new ErrorModel();
        }

        [JsonProperty("accept_partial")]
        public bool AcceptPartial { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("amount_paid")]
        public int AmountPaid { get; set; }

        [JsonProperty("callback_method")]
        public string CallbackMethod { get; set; }

        [JsonProperty("callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty("cancelled_at")]
        public int CancelledAt { get; set; }

        [JsonProperty("created_at")]
        public int CreatedAt { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("customer")]
        public CustomerModel Customer { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("expire_by")]
        public int ExpireBy { get; set; }

        [JsonProperty("expired_at")]
        public int ExpiredAt { get; set; }

        [JsonProperty("first_min_partial_amount")]
        public int FirstMinPartialAmount { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("notes")]
        public IDictionary<string, object> Notes { get; set; }

        [JsonProperty("notify")]
        public NotifyModel Notify { get; set; }

        [JsonProperty("payments")]
        public object Payments { get; set; }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("reminder_enable")]
        public bool ReminderEnable { get; set; }

        [JsonProperty("reminders")]
        public List<object> Reminders { get; set; }

        [JsonProperty("short_url")]
        public string ShortUrl { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("updated_at")]
        public int UpdatedAt { get; set; }

        [JsonProperty("upi_link")]
        public bool UpiLink { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
