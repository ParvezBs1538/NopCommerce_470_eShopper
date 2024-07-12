using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.SMS.TeleSign.Domains;

namespace NopStation.Plugin.SMS.TeleSign.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(SmsTemplate), "NS_TeleSign_SmsTemplate" },
            { typeof(QueuedSms), "NS_TeleSign_QueuedSms" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
