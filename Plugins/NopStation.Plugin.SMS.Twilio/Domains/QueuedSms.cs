using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NopStation.Plugin.SMS.Twilio.Domains
{
    public class QueuedSms : BaseEntity
    {
        public int? CustomerId { get; set; }

        public int StoreId { get; set; }

        public string Body { get; set; }

        public string PhoneNumber { get; set; }

        public int SentTries { get; set; }

        public string Error { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? SentOnUtc { get; set; }
    }
}
