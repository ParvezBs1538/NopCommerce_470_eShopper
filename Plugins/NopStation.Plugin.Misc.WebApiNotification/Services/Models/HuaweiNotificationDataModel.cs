using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Models
{
    public class HuaweiNotificationDataModel
    {
        [JsonProperty("notification_foreground")]
        public string NotificationForeground { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("itemType")]
        public string ActionType { get; set; }

        [JsonProperty("itemId")]
        public string ActionValue { get; set; }

        [JsonProperty("bigPicture")]
        public string BigPicture { get; set; }
    }
}
