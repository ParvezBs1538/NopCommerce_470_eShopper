using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NopStation.Plugin.SMS.Vonage.Domains
{
    public class QueuedSms : BaseEntity
    {
        public int? CustomerId { get; set; }

        public int StoreId { get; set; }

        public string Body { get; set; }

        public string PhoneNumber { get; set; }

        public int SentTries { get; set; }

        public string Error { get; set; }

        public string RemainingBalance { get; set; }

        public string MessagePrice { get; set; }

        public string Network { get; set; }

        public string AccountRef { get; set; }

        public string MessageId { get; set; }

        public string MessageCount { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? SentOnUtc { get; set; }
    }
}
