using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.SMS.Twilio.Domains;

namespace NopStation.Plugin.SMS.Twilio.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(SmsTemplate), "NS_Twilio_SmsTemplate" },
            { typeof(QueuedSms), "NS_Twilio_QueuedSms" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
