using Nop.Core.Configuration;
using NopStation.Plugin.Payments.Unzer.Domain;

namespace NopStation.Plugin.Payments.Unzer
{
    public class UnzerPaymentSettings : ISettings
    {
        public string ApiPrivateKey { get; set; }

        public string ApiPublicKey { get; set; }

        public string ApiEndpoint { get; set; }

        public string ApiVersion { get; set; }

        public bool UseSandbox { get; set; }

        public int TransactionModeId { get; set; }

        public TransactionMode TransactionMode { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public bool IsCardActive { get; set; }

        public bool IsPaypalActive { get; set; }

        public bool IsSofortActive { get; set; }

        public bool IsEpsActive { get; set; }
    }
}