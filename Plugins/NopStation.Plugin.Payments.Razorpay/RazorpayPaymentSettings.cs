using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Razorpay
{
    public class RazorpayPaymentSettings : ISettings
    {
        public string KeyId { get; set; }

        public string KeySecret { get; set; }

        public string Description { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
