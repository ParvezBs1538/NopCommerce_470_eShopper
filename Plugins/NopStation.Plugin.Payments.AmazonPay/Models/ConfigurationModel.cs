using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.AmazonPay.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
            AvailableRegions = new List<SelectListItem>();
            AvailableButtonColors = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.DescriptionText")]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.PrivateKey")]
        public string PrivateKey { get; set; }
        public bool PrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.PublicKeyId")]
        public string PublicKeyId { get; set; }
        public bool PublicKeyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.NoteToBuyer")]
        public string NoteToBuyer { get; set; }
        public bool NoteToBuyer_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.ButtonColor")]
        public string ButtonColor { get; set; }
        public bool ButtonColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.StoreId")]
        public string StoreId { get; set; }
        public bool StoreId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.Region")]
        public int RegionId { get; set; }
        public bool RegionId_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }
        public IList<SelectListItem> AvailableRegions { get; set; }
        public IList<SelectListItem> AvailableButtonColors { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.AmazonPay.Configuration.Fields.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion
    }
}
