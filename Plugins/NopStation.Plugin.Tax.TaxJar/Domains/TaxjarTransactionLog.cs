using System;
using Nop.Core;

namespace NopStation.Plugin.Tax.TaxJar.Domains
{
    public class TaxjarTransactionLog : BaseEntity
    {
        public string TransactionId { get; set; }

        public string TransactionReferanceId { get; set; }

        public string TransactionType { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public decimal OrderTaxAmount { get; set; }

        public int CustomerId { get; set; }

        public string TransactionDate { get; set; }

        public DateTime? CreatedDateUtc { get; set; }

        public DateTime? UpdatedDateUtc { get; set; }
    }
}
