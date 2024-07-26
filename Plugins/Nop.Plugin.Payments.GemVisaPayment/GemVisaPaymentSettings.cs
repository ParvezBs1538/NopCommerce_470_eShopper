using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.GemVisaPayment
{
    /// <summary>
    /// Represents settings of the GemVisa Standard payment plugin
    /// </summary>
    public class GemVisaPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public bool DebugMode { get; set; }
        public string MerchantId { get; set; }
        public string SecretKey { get; set; }
        public int MinimumAmount { get; set; }
        public int MaximumAmount { get; set; }
        public bool ShowInfoInProductDetailsPage { get; set; }
        public int InstallmentMonth { get; set; }
    }
}
