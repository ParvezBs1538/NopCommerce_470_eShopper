using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.EveryPay
{
    public class EveryPaySettings : ISettings
    {
        public int TransactModeId { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string ApiKey { get; set; }

        public string PrivateKey { get; set; }

        public string Installments { get; set; }

        public bool UseSandbox { get; set; }
    }
}
