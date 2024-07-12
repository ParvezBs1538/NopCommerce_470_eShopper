using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.StripeKonbini.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableCurrencyCodes = new List<SelectListItem>();
            SupportedCurrencyCodes = new List<string>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.PublishableKey")]
        public string PublishableKey { get; set; }
        public bool PublishableKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.WebhookSecret")]
        public string WebhookSecret { get; set; }
        public bool WebhookSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.SendOrderInfoToStripe")]
        public bool SendOrderInfoToStripe { get; set; }
        public bool SendOrderInfoToStripe_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeKonbini.Configuration.Fields.SupportedCurrencyCodes")]
        public IList<string> SupportedCurrencyCodes { get; set; }
        public bool SupportedCurrencyCodes_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCurrencyCodes { get; set; }
    }
}
