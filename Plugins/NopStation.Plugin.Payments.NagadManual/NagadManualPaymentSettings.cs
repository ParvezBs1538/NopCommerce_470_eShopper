using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.NagadManual
{
    public class NagadManualPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string NagadNumber { get; set; }

        public string NumberType { get; set; }

        public bool ValidatePhoneNumber { get; set; }

        public string PhoneNumberRegex { get; set; }
    }
}
