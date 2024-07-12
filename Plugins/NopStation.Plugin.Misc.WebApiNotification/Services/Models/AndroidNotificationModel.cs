using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Models
{
    public class AndroidNotificationModel : NotificationBaseModel
    {
        public AndroidNotificationModel()
        {
            Data = new DataModel();
        }

        [JsonProperty("data")]
        public DataModel Data { get; set; }

        public partial class DataModel
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("itemType")]
            public int ActionType { get; set; }

            [JsonProperty("itemId")]
            public string ActionValue { get; set; }

            [JsonProperty("bigPicture")]
            public string BigPicture { get; set; }
        }
    }
}
