using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Instamojo.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.PrivateApiKey")]
        public string PrivateApiKey { get; set; }
        public bool PrivateApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.PrivateAuthToken")]
        public string PrivateAuthToken { get; set; }
        public bool PrivateAuthToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.PrivateSalt")]
        public string PrivateSalt { get; set; }
        public bool PrivateSalt_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.EnableSendSMS")]
        public bool EnableSendSMS { get; set; }
        public bool EnableSendSMS_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.EnableSendEmail")]
        public bool EnableSendEmail { get; set; }
        public bool EnableSendEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.Instamojo.Configuration.Fields.Description")]
            public string Description { get; set; }
        }

        #endregion
    }
}
