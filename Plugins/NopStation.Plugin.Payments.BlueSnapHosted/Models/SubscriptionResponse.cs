using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class SubscriptionResponse
    {
        public SubscriptionResponse()
        {
            Charge = new Charge();
        }

        [JsonProperty("subscriptionId")]
        public int SubscriptionId { get; set; }

        [JsonProperty("planId")]
        public int PlanId { get; set; }

        [JsonProperty("vaultedShopperId")]
        public int VaultedShopperId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("softDescriptor")]
        public string SoftDescriptor { get; set; }

        [JsonProperty("chargeFrequency")]
        public string ChargeFrequency { get; set; }

        [JsonProperty("recurringChargeAmount")]
        public decimal RecurringChargeAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("autoRenew")]
        public bool AutoRenew { get; set; }

        [JsonProperty("nextChargeDate")]
        public string NextChargeDate { get; set; }

        [JsonProperty("payerInfo")]
        public PayerInfo PayerInfo { get; set; }

        [JsonProperty("paymentSource")]
        public PaymentSource PaymentSource { get; set; }

        [JsonProperty("charge")]
        public Charge Charge { get; set; }

        [JsonProperty("fraudResultInfo")]
        public FraudResultInfo FraudResultInfo { get; set; }
    }
    public class Charge
    {
        [JsonProperty("transactionId")]
        public string TransactionId { set; get; }
    }
    public class FraudResultInfo
    {
    }
}
