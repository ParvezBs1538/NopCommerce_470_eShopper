using Newtonsoft.Json;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services.Models
{
    public class PushNotificationModel
    {
        public PushNotificationModel()
        {
            Data = new DataModel();
        }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string IconUrl { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "vibrate")]
        public string Vibration { get; set; }

        [JsonProperty(PropertyName = "sound")]
        public string SoundFileUrl { get; set; }

        [JsonProperty(PropertyName = "dir")]
        public string Direction { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }

        public class DataModel
        {
            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }
    }
}
