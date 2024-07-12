using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.StripeAlipay.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
            SupportedCurrencyCodes = new List<string>();
            AvailableCurrencyCodes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.DescriptionText")]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.PublishableKey")]
        public string PublishableKey { get; set; }
        public bool PublishableKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.EnableWebhook")]
        public bool EnableWebhook { get; set; }
        public bool EnableWebhook_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.WebhookSecret")]
        public string WebhookSecret { get; set; }
        public bool WebhookSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.SupportedCurrencyCodes")]
        public IList<string> SupportedCurrencyCodes { get; set; }
        public bool SupportedCurrencyCodes_OverrideForStore { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }
        public IList<SelectListItem> AvailableCurrencyCodes { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.StripeAlipay.Configuration.Fields.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion
    }
}
