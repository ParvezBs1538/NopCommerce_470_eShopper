using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Affirm.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.PublicApiKey")]
        public string PublicApiKey { get; set; }
        public bool PublicApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.PrivateApiKey")]
        public string PrivateApiKey { get; set; }
        public bool PrivateApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.FinancialProductKey")]
        public string FinancialProductKey { get; set; }
        public bool FinancialProductKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.MerchantName")]
        public string MerchantName { get; set; }
        public bool MerchantName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.EnableOnProductBox")]
        public bool EnableOnProductBox { get; set; }
        public bool EnableOnProductBox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.TransactionMode")]
        public int TransactionModeId { get; set; }
        public bool TransactionModeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.CountryAPIMode")]
        public int CountryAPIModeId { get; set; }
        public bool CountryAPIModeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffirmPayment.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
        public SelectList AvailableTransactionModes { get; set; }
        public SelectList AvailableCountryAPIModes { get; set; }
    }
}
