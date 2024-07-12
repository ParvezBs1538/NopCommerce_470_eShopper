using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.RocketManual.Models
{
    public record ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
            AvailableNumberTypes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.DescriptionText")]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.RocketNumber")]
        public string RocketNumber { get; set; }
        public bool RocketNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.NumberType")]
        public string NumberType { get; set; }
        public bool NumberType_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.ValidatePhoneNumber")]
        public bool ValidatePhoneNumber { get; set; }
        public bool ValidatePhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        public IList<ConfigurationLocalizedModel> Locales { get; set; }
        public IList<SelectListItem> AvailableNumberTypes { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.NopStation.RocketManual.Configuration.Fields.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion
    }
}