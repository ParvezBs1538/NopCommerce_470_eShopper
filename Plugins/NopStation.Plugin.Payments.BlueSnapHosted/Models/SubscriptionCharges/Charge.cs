using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class Charge
    {
        [JsonProperty("chargeId")]
        public int ChargeId { get; set; }
        [JsonProperty("subscriptionId")]
        public int SubscriptionId { get; set; }
        [JsonProperty("planId")]
        public int PlanId { get; set; }
        [JsonProperty("vaultedShopperId")]
        public int VaultedShopperId { get; set; }
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
        [JsonProperty("transactionDate")]
        public string TransactionDate { get; set; }
        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("softDescriptor")]
        public string SoftDescriptor { get; set; }
        [JsonProperty("paymentSource")]
        public PaymentSource PaymentSource { get; set; }
        [JsonProperty("chargeInfo")]
        public ChargeInfo ChargeInfo { get; set; }
        [JsonProperty("processingInfo")]
        public ProcessingInfo ProcessingInfo { get; set; }
    }
}
