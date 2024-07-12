using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.AffiliateStation.Domains
{
    public partial class AffiliateCustomer : BaseEntity
    {
        public int AffiliateId { get; set; }

        public int CustomerId { get; set; }

        public int ApplyStatusId { get; set; }

        public bool OverrideCatalogCommission { get; set; }

        public decimal CommissionAmount { get; set; }

        public bool UsePercentage { get; set; }

        public decimal CommissionPercentage { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public ApplyStatus ApplyStatus
        {
            get => (ApplyStatus)ApplyStatusId;
            set => ApplyStatusId = (int)value;
        }
    }
}
