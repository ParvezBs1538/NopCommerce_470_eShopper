using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(SmartGroup), "NS_PWA_SmartGroup" },
            { typeof(SmartGroupCondition), "NS_PWA_SmartGroupCondition" },
            { typeof(SmartGroupNotification), "NS_PWA_SmartGroupNotification" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
