using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.HideMethodIfInsufficientBalance")]
        public bool HideMethodIfInsufficientBalance { get; set; }
        public bool HideMethodIfInsufficientBalance_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.SkipPaymentInfo")]
        public bool SkipPaymentInfo { get; set; }
        public bool SkipPaymentInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.DescriptionText")]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.ShowAvailableCreditOnCheckoutPage")]
        public bool ShowAvailableCreditOnCheckoutPage { get; set; }
        public bool ShowAvailableCreditOnCheckoutPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.ShowInvoicesInCustomerWalletPage")]
        public bool ShowInvoicesInCustomerWalletPage { get; set; }
        public bool ShowInvoicesInCustomerWalletPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.MaxInvoicesToShowInCustomerWalletPage")]
        public int MaxInvoicesToShowInCustomerWalletPage { get; set; }
        public bool MaxInvoicesToShowInCustomerWalletPage_OverrideForStore { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.CreditWallet.Configuration.Fields.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion
    }
}
