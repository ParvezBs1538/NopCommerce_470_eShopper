using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Models
{
    public class IOsNotificationModel : NotificationBaseModel
    {
        public IOsNotificationModel()
        {
            Notification = new NotificationModel();
            Data = new DataModel();
        }

        [JsonProperty("notification")]
        public NotificationModel Notification { get; set; }

        [JsonProperty("data")]
        public DataModel Data { get; set; }

        public partial class DataModel
        {
            [JsonProperty("itemType")]
            public int ActionType { get; set; }

            [JsonProperty("itemId")]
            public string ActionValue { get; set; }

            [JsonProperty("bigPicture")]
            public string ImageUrl { get; set; }
        }

        public partial class NotificationModel
        {
            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("badge")]
            public long Badge { get; set; }

            [JsonProperty("sound")]
            public string Sound { get; set; }

            [JsonProperty("mutable_content")]
            public bool MutableContent { get; set; }
        }
    }
}
