using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Razorpay.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.NopStation.Razorpay.Configuration.Fields.KeyId")]
        public string KeyId { get; set; }
        public bool KeyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Razorpay.Configuration.Fields.KeySecret")]
        public string KeySecret { get; set; }
        public bool KeySecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Razorpay.Configuration.Fields.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Razorpay.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Razorpay.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.Razorpay.Configuration.Fields.Description")]
            public string Description { get; set; }
        }

        #endregion
    }
}
