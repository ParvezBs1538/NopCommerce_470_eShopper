using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.SMS.Messente.Domains;

namespace NopStation.Plugin.SMS.Messente.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(SmsTemplate), "NS_Messente_SmsTemplate" },
            { typeof(QueuedSms), "NS_Messente_QueuedSms" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
        };
    }
}
