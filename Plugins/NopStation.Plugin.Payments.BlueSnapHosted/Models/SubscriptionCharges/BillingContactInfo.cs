using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class BillingContactInfo
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
    }
}
