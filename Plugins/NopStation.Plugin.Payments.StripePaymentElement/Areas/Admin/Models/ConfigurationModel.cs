using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.StripePaymentElement.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    public ConfigurationModel()
    {
        TransactionModeValues = new List<SelectListItem>();
        ThemeValues = new List<SelectListItem>();
        LayoutValues = new List<SelectListItem>();
    }

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }
    public bool AdditionalFeePercentage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.AdditionalFee")]
    public decimal AdditionalFee { get; set; }
    public bool AdditionalFee_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.TransactionMode")]
    public int TransactionModeId { get; set; }
    public bool TransactionModeId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.Theme")]
    public string Theme { get; set; }
    public bool Theme_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.Layout")]
    public string Layout { get; set; }
    public bool Layout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.ApiKey")]
    public string ApiKey { get; set; }
    public bool ApiKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.PublishableKey")]
    public string PublishableKey { get; set; }
    public bool PublishableKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.EnableLogging")]
    public bool EnableLogging { get; set; }
    public bool EnableLogging_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.StripePaymentElement.Configuration.Fields.AppleVerificationFileExist")]
    public bool AppleVerificationFileExist { get; set; }

    public IList<SelectListItem> TransactionModeValues { get; set; }
    public IList<SelectListItem> ThemeValues { get; set; }
    public IList<SelectListItem> LayoutValues { get; set; }
}
