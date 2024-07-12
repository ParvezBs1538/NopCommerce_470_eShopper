using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.MPay24
{
    public class MPay24PaymentSettings : ISettings
    {
        public string SoapUsername { get; set; }

        public string SoapPassword { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public int MerchantId { get; set; }

        public bool Sandbox { get; set; }
    }
}
