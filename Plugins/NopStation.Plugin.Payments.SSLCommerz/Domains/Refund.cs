using System;
using Nop.Core;

namespace NopStation.Plugin.Payments.SSLCommerz.Domains
{
    public class Refund : BaseEntity
    {
        public int OrderId { get; set; }

        public string RefrenceId { get; set; }

        public bool Refunded { get; set; }

        public int StatusCheckCount { get; set; }

        public DateTime? InitiatedOn { get; set; }

        public DateTime? RefundedOn { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public decimal RefundAmount { get; set; }
    }
}
