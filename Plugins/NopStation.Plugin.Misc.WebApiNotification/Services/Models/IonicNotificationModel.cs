using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Models
{
    public class IonicNotificationModel : NotificationBaseModel
    {
        public IonicNotificationModel()
        {
            Notification = new NotificationModel();
            Data = new DataModel();
        }

        [JsonProperty("notification")]
        public NotificationModel Notification { get; set; }

        [JsonProperty("data")]
        public DataModel Data { get; set; }

        public class NotificationModel
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("image")]
            public string Image { get; set; }
        }

        public class DataModel
        {
            [JsonProperty("notification_foreground")]
            public string NotificationForeground { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("itemType")]
            public int ActionType { get; set; }

            [JsonProperty("itemId")]
            public string ActionValue { get; set; }

            [JsonProperty("bigPicture")]
            public string ImageUrl { get; set; }
        }
    }
}
