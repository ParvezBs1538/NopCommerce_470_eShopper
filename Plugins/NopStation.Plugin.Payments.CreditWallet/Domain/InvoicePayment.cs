using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Payments.CreditWallet.Domain
{
    public class InvoicePayment : BaseEntity, ISoftDeletedEntity
    {
        public int WalletCustomerId { get; set; }

        public string InvoiceReference { get; set; }

        public DateTime PaymentDateUtc { get; set; }

        public decimal Amount { get; set; }

        public int CreatedByCustomerId { get; set; }

        public int UpdatedByCustomerId { get; set; }

        public string Note { get; set; }

        public bool Deleted { get; set; }
    }
}