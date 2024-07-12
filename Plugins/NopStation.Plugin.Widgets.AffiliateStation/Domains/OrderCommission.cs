using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.AffiliateStation.Domains
{
    public partial class OrderCommission : BaseEntity
    {
        public int OrderId { get; set; }

        public decimal TotalCommissionAmount { get; set; }

        public int CommissionStatusId { get; set; }

        public DateTime? CommissionPaidOn { get; set; }

        public decimal PartialPaidAmount { get; set; }

        public int AffiliateId { get; set; }

        public CommissionStatus CommissionStatus
        {
            get => (CommissionStatus)CommissionStatusId;
            set => CommissionStatusId = (int)value;
        }
    }
}
