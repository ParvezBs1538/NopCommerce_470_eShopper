using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Instamojo.Models
{
    public class PaymentInitResponseModel
    {
        public PaymentInitResponseModel()
        {
            PaymentRequest = new PaymentRequestModel();
        }

        [JsonProperty("payment_request")]
        public PaymentRequestModel PaymentRequest { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        public class PaymentRequestModel
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("buyer_name")]
            public string BuyerName { get; set; }

            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("purpose")]
            public string Purpose { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("send_sms")]
            public bool EnableSendSMS { get; set; }

            [JsonProperty("send_email")]
            public bool EnableSendEmail { get; set; }

            [JsonProperty("sms_status")]
            public string SmsStatus { get; set; }

            [JsonProperty("email_status")]
            public string EmailStatus { get; set; }

            [JsonProperty("longurl")]
            public string LongUrl { get; set; }

            [JsonProperty("redirect_url")]
            public string RedirectUrl { get; set; }

            [JsonProperty("webhook")]
            public string Webhook { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("modified_at")]
            public DateTime ModifiedAt { get; set; }

            [JsonProperty("allow_repeated_payments")]
            public bool AllowRepeatedPayments { get; set; }
        }
    }
}
