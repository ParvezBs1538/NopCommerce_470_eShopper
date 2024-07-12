using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Models
{
    public class NotificationBaseModel
    {
        [JsonProperty("to")]
        public string SubscriptionId { get; set; }
    }
}
