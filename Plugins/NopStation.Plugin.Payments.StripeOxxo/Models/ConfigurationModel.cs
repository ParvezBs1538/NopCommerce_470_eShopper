using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.StripeOxxo.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
            AvailableCurrencyCodes = new List<SelectListItem>();
            SupportedCurrencyCodes = new List<string>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.DescriptionText")]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.PublishableKey")]
        public string PublishableKey { get; set; }
        public bool PublishableKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.WebhookSecret")]
        public string WebhookSecret { get; set; }
        public bool WebhookSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.SendOrderInfoToStripe")]
        public bool SendOrderInfoToStripe { get; set; }
        public bool SendOrderInfoToStripe_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.SupportedCurrencyCodes")]
        public IList<string> SupportedCurrencyCodes { get; set; }
        public bool SupportedCurrencyCodes_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCurrencyCodes { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.StripeOxxo.Configuration.Fields.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion
    }
}
