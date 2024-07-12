using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(PushNotificationAnnouncement), "NS_PWA_PushNotificationAnnouncement" },
            { typeof(PushNotificationTemplate), "NS_PWA_PushNotificationTemplate" },
            { typeof(QueuedPushNotification), "NS_PWA_QueuedPushNotification" },
            { typeof(WebAppDevice), "NS_PWA_AppDevice" },
            { typeof(AbandonedCartTracking), "NS_PWA_AbandonedCartTracking" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
