using Nop.Core;

namespace NopStation.Plugin.Widgets.AffiliateStation.Domains
{
    public class CatalogCommission : BaseEntity
    {
        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public decimal CommissionAmount { get; set; }

        public bool UsePercentage { get; set; }

        public decimal CommissionPercentage { get; set; }
    }
}
