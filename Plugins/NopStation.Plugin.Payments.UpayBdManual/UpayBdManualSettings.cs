using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.UpayBdManual
{
    public class UpayBdManualSettings : ISettings
    {
        public string DescriptionText { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string UpayNumber { get; set; }

        public string NumberType { get; set; }

        public bool ValidatePhoneNumber { get; set; }

        public string PhoneNumberRegex { get; set; }
    }
}
