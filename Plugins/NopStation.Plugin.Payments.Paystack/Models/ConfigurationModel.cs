using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Paystack.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
            Channels = new List<string>();
            Currencies = new List<string>();
            AvailableChannels = new List<SelectListItem>();
            AvailableCurrencies = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.PublicKey")]
        public string PublicKey { get; set; }
        public bool PublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.Channels")]
        public IList<string> Channels { get; set; }
        public bool Channels_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.Currencies")]
        public IList<string> Currencies { get; set; }
        public bool Currencies_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }
        public IList<SelectListItem> AvailableChannels { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.Paystack.Configuration.Fields.Description")]
            public string Description { get; set; }
        }

        #endregion
    }
}
