using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BlueSnapHosted.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BlueSnapHosted.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Sandbox")]
        public bool IsSandBox { get; set; }
        public bool IsSandBox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Username")]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }
    }
}