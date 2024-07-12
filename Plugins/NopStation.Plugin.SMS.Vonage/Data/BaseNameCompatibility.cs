using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.SMS.Vonage.Domains;

namespace NopStation.Plugin.SMS.Vonage.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(SmsTemplate), "NS_Vonage_SmsTemplate" },
            { typeof(QueuedSms), "NS_Vonage_QueuedSms" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
