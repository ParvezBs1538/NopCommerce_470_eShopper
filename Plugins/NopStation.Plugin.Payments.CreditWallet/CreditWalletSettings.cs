using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.CreditWallet
{
    public class CreditWalletSettings : ISettings
    {
        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public string DescriptionText { get; set; }

        public bool SkipPaymentInfo { get; set; }

        public bool HideMethodIfInsufficientBalance { get; set; }

        public bool ShowInvoicesInCustomerWalletPage { get; set; }

        public int MaxInvoicesToShowInCustomerWalletPage { get; set; }

        public bool ShowAvailableCreditOnCheckoutPage { get; set; }
    }
}
