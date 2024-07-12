using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.Configuration.Fields.UseDefaultCommissionIfNotSetOnCatalog")]
        public bool UseDefaultCommissionIfNotSetOnCatalog { get; set; }
        public bool UseDefaultCommissionIfNotSetOnCatalog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.Configuration.Fields.CommissionAmount")]
        public decimal CommissionAmount { get; set; }
        public bool CommissionAmount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.Configuration.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }
        public bool UsePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.Configuration.Fields.CommissionPercentage")]
        public decimal CommissionPercentage { get; set; }
        public bool CommissionPercentage_OverrideForStore { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.Configuration.Fields.AffiliatePageOrderPageSize")]
        public int AffiliatePageOrderPageSize { get; set; }
        public bool AffiliatePageOrderPageSize_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
