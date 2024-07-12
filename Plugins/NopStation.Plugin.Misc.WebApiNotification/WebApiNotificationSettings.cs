using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.WebApiNotification
{
    public class WebApiNotificationSettings : ISettings
    {
        public string GoogleConsoleApiAccessKey { get; set; }

        public int ApplicationTypeId { get; set; }

        public string PushKitClientSecret { get; set; }

        public string PushKitClientId { get; set; }

        public string PushKitAppId { get; set; }
    }
}
