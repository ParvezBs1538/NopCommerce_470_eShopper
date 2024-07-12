using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public record AppLandingConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.WebApi.AppLandingConfigurationModel.Fields.EnableFeatureProducts")]
        public bool EnableFeatureProducts { get; set; }
        [NopResourceDisplayName("Admin.NopStation.WebApi.AppLandingConfigurationModel.Fields.EnableBestSellingProducts")]
        public bool EnableBestSellingProducts { get; set; }
        [NopResourceDisplayName("Admin.NopStation.WebApi.AppLandingConfigurationModel.Fields.EnableHomeCategoriesProducts")]
        public bool EnableHomeCategoriesProducts { get; set; }
        [NopResourceDisplayName("Admin.NopStation.WebApi.AppLandingConfigurationModel.Fields.EnableSubCategoriesProducts")]
        public bool EnableSubCategoriesProducts { get; set; }
        [NopResourceDisplayName("Admin.NopStation.WebApi.AppLandingConfigurationModel.Fields.NumberOfHomeCategoriesProducts")]
        public int NumberOfHomeCategoriesProducts { get; set; }
        [NopResourceDisplayName("Admin.NopStation.WebApi.AppLandingConfigurationModel.Fields.NumberOfManufaturer")]
        public int NumberOfManufaturer { get; set; }
        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
