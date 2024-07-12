using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.AffiliateStation
{
    public class AffiliateStationSettings : ISettings
    {
        public bool UseDefaultCommissionIfNotSetOnCatalog { get; set; }

        public decimal CommissionAmount { get; set; }

        public bool UsePercentage { get; set; }

        public decimal CommissionPercentage { get; set; }

        public int AffiliatePageOrderPageSize { get; set; }
    }
}
