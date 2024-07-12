using System;
using Nop.Core;

namespace NopStation.Plugin.Payments.Dmoney.Domains
{
    public class DmoneyTransaction : BaseEntity
    {
        public int StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public string MerchantWalletNumber { get; set; }

        public string CustomerWalletNumber { get; set; }

        public decimal Amount { get; set; }

        public string TransactionType { get; set; }

        public int OrderId { get; set; }

        public string TransactionTime { get; set; }

        public string PaymentStatus { get; set; }

        public string TransactionReferenceId { get; set; }

        public string TransactionTrackingNumber { get; set; }

        public string StatusMessage { get; set; }

        public string ErrorCode { get; set; }

        public TransactionStatus TransactionStatus { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime LastUpdatedOnUtc { get; set; }
    }
}
