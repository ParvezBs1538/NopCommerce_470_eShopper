using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.AamarPay;

public class AamarPayPaymentSettings : ISettings
{
    public bool UseSandbox { get; set; }
    public string MerchantId { get; set; }
    public string SignatureKey { get; set; }
    public bool AdditionalFeePercentage { get; set; }
    public decimal AdditionalFee { get; set; }
}
