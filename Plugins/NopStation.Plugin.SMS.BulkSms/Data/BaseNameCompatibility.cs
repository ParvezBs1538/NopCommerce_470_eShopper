using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.SMS.BulkSms.Domains;

namespace NopStation.Plugin.SMS.BulkSms.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(SmsTemplate), "NS_BulkSms_SmsTemplate" },
            { typeof(QueuedSms), "NS_BulkSms_QueuedSms" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
        };
    }
}
