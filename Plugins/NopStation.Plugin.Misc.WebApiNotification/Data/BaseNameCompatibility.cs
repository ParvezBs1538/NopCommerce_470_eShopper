using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(WebApiNotificationCampaign), "NS_WebApi_NotificationCampaign" },
            { typeof(WebApiNotificationTemplate), "NS_WebApi_NotificationTemplate" },
            { typeof(WebApiQueuedNotification), "NS_WebApi_QueuedNotification" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
