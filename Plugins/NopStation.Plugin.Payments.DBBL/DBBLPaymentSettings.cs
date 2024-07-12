using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.DBBL
{
    public class DBBLPaymentSettings : ISettings
    {
        public string UserId { get; set; }

        public string Password { get; set; }

        public bool UseSandbox { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
