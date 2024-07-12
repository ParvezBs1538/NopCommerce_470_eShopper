using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Models
{
    public class HuaweiNotificationModel : NotificationBaseModel
    {
        [JsonProperty("validate_only")]
        public bool ValidateOnly { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public long? Expires { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("error")]
        public long? Error { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }

        [JsonIgnore]
        private DateTime _createTime;

        public TokenResponse()
        {
            _createTime = DateTime.UtcNow;
        }

        private static TimeSpan _timeInAdvance = TimeSpan.FromMinutes(5);

        internal string GetValidAccessToken()
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                return null;
            }

            var leftDuration = _createTime.AddSeconds(Expires ?? 0) - DateTime.UtcNow;
            if (leftDuration < _timeInAdvance)
            {
                return null;
            }

            return AccessToken;
        }
    }

    public class AndroidConfig
    {
        [JsonProperty("urgency")]
        public string Urgency { get; set; }

        [JsonProperty("ttl")]
        public string Ttl { get; set; }

        [JsonProperty("notification")]
        public Notification Notification { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("bi_tag")]
        public string BITag { get; set; }
    }

    public class ClickAction
    {
        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public class Message
    {
        //[JsonProperty("notification")]
        //public Notification Notification { get; set; }

        //[JsonProperty("android")]
        //public AndroidConfig Android { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("token")]
        public IList<string> Token { get; set; }
    }

    public class Notification
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("click_action")]
        public ClickAction ClickAction { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class SingleMessageResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }
    }

}
