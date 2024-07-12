using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.AamarPay.Areas.Admin.Models;

public class ConfigurationModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AamarPay.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }
    public bool UseSandbox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AamarPay.Fields.MerchantId")]
    public string MerchantId { get; set; }
    public bool MerchantId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AamarPay.Fields.SignatureKey")]
    public string SignatureKey { get; set; }
    public bool SignatureKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AamarPay.Fields.AdditionalFee")]
    public decimal AdditionalFee { get; set; }
    public bool AdditionalFee_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AamarPay.Fields.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }
    public bool AdditionalFeePercentage_OverrideForStore { get; set; }
}
