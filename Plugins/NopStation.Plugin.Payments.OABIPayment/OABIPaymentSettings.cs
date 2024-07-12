using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.OABIPayment
{
    public class OABIPaymentSettings : ISettings
    {
        public string TranPortalId { get; set; }
        public string TranPortaPassword { get; set; }
        public string ResourceKey { get; set; }
        public bool AdditionalFeeInPercentage { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
