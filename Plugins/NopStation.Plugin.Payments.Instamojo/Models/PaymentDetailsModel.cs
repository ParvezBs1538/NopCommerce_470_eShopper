using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Instamojo.Models
{
    public class PaymentDetailsModel
    {
        public PaymentDetailsModel()
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
            public decimal Amount { get; set; }

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

            [JsonProperty("shorturl")]
            public string Shorturl { get; set; }

            [JsonProperty("longurl")]
            public string Longurl { get; set; }

            [JsonProperty("redirect_url")]
            public string RedirectUrl { get; set; }

            [JsonProperty("webhook")]
            public string Webhook { get; set; }

            [JsonProperty("payments")]
            public List<PaymentModel> Payments { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("modified_at")]
            public DateTime ModifiedAt { get; set; }

            [JsonProperty("allow_repeated_payments")]
            public bool AllowRepeatedPayments { get; set; }
        }

        public class PaymentModel
        {
            public PaymentModel()
            {
                Payout = new PayoutModel();
            }

            [JsonProperty("payment_id")]
            public string PaymentId { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("link_slug")]
            public object LinkSlug { get; set; }

            [JsonProperty("link_title")]
            public object LinkTitle { get; set; }

            [JsonProperty("buyer_name")]
            public string BuyerName { get; set; }

            [JsonProperty("buyer_phone")]
            public string BuyerPhone { get; set; }

            [JsonProperty("buyer_email")]
            public string BuyerEmail { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("unit_price")]
            public string UnitPrice { get; set; }

            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("fees")]
            public string Fees { get; set; }

            [JsonProperty("shipping_address")]
            public object ShippingAddress { get; set; }

            [JsonProperty("shipping_city")]
            public object ShippingCity { get; set; }

            [JsonProperty("shipping_state")]
            public object ShippingState { get; set; }

            [JsonProperty("shipping_zip")]
            public object ShippingZip { get; set; }

            [JsonProperty("shipping_country")]
            public object ShippingCountry { get; set; }

            [JsonProperty("discount_code")]
            public object DiscountCode { get; set; }

            [JsonProperty("discount_amount_off")]
            public object DiscountAmountOff { get; set; }

            [JsonProperty("affiliate_id")]
            public object AffiliateId { get; set; }

            [JsonProperty("affiliate_commission")]
            public string AffiliateCommission { get; set; }

            [JsonProperty("instrument_type")]
            public string InstrumentType { get; set; }

            [JsonProperty("billing_instrument")]
            public string BillingInstrument { get; set; }

            [JsonProperty("failure")]
            public object Failure { get; set; }

            [JsonProperty("payout")]
            public PayoutModel Payout { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("payment_request")]
            public string PaymentRequest { get; set; }
        }

        public class PayoutModel
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("paid_out_at")]
            public DateTime PaidOutAt { get; set; }
        }
    }
}