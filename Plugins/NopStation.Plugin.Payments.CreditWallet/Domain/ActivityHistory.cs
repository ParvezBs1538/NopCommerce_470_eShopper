using System;
using Nop.Core;

namespace NopStation.Plugin.Payments.CreditWallet.Domain
{
    public class ActivityHistory : BaseEntity
    {
        public int WalletCustomerId { get; set; }

        public int ActivityTypeId { get; set; }

        public decimal PreviousTotalCreditUsed { get; set; }

        public decimal CurrentTotalCreditUsed { get; set; }

        public int CreatedByCustomerId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string Note { get; set; }

        public ActivityType ActivityType
        {
            get => (ActivityType)ActivityTypeId;
            set => ActivityTypeId = (int)value;
        }
    }
}
