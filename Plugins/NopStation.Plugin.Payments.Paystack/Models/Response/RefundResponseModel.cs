using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paystack.Models.Response
{
    public class RefundResponseModel
    {
        public RefundResponseModel()
        {
            Data = new DataModel();
        }

        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public DataModel Data { get; set; }

        public class TransactionModel
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("domain")]
            public string Domain { get; set; }

            [JsonProperty("reference")]
            public string Reference { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }

            [JsonProperty("paid_at")]
            public DateTime PaidAt { get; set; }

            [JsonProperty("channel")]
            public string Channel { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("order_id")]
            public object OrderId { get; set; }

            [JsonProperty("pos_transaction_data")]
            public object PosTransactionData { get; set; }

            [JsonProperty("source")]
            public object Source { get; set; }

            [JsonProperty("fees_breakdown")]
            public object FeesBreakdown { get; set; }
        }

        public class DataModel
        {
            public DataModel()
            {
                Transaction = new TransactionModel();
            }

            [JsonProperty("transaction")]
            public TransactionModel Transaction { get; set; }

            [JsonProperty("integration")]
            public int Integration { get; set; }

            [JsonProperty("deducted_amount")]
            public int DeductedAmount { get; set; }

            [JsonProperty("channel")]
            public object Channel { get; set; }

            [JsonProperty("merchant_note")]
            public string MerchantNote { get; set; }

            [JsonProperty("customer_note")]
            public string CustomerNote { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("refunded_by")]
            public string RefundedBy { get; set; }

            [JsonProperty("expected_at")]
            public DateTime ExpectedAt { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("domain")]
            public string Domain { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }

            [JsonProperty("fully_deducted")]
            public bool FullyDeducted { get; set; }

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("createdAt")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("updatedAt")]
            public DateTime UpdatedAt { get; set; }
        }
    }
}
