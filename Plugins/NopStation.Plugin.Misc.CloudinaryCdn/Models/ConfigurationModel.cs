using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CloudinaryCdn.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiSecret")]
        public string ApiSecret { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.CloudName")]
        public string CloudName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.PrependCdnFolderName")]
        public bool PrependCdnFolderName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.CdnFolderName")]
        public string CdnFolderName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.EnableJsCdn")]
        public bool EnableJsCdn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.EnableCssCdn")]
        public bool EnableCssCdn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateRequiredPictures")]
        public bool AutoGenerateRequiredPictures { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductThumbPicture")]
        public bool AutoGenerateProductThumbPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductDetailsPicture")]
        public bool AutoGenerateProductDetailsPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductThumbPictureOnProductDetailsPage")]
        public bool AutoGenerateProductThumbPictureOnProductDetailsPage { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateAssociatedProductPicture")]
        public bool AutoGenerateAssociatedProductPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateCategoryThumbPicture")]
        public bool AutoGenerateCategoryThumbPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateManufacturerThumbPicture")]
        public bool AutoGenerateManufacturerThumbPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateVendorThumbPicture")]
        public bool AutoGenerateVendorThumbPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateCartThumbPicture")]
        public bool AutoGenerateCartThumbPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateMiniCartThumbPicture")]
        public bool AutoGenerateMiniCartThumbPicture { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateAutoCompleteSearchThumbPicture")]
        public bool AutoGenerateAutoCompleteSearchThumbPicture { get; set; }
    }
}
