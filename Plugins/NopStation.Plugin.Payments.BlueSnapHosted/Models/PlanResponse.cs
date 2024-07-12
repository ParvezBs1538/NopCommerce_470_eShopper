using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class PlanResponse
    {
        [JsonProperty("chargeFrequency")]
        public string ChargeFrequency { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("planId")]
        public int PlanId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("recurringChargeAmount")]
        public decimal RecurringChargeAmount { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
