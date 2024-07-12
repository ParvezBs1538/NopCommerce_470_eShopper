using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.CloudinaryCdn
{
    public class CloudinaryCdnSettings : ISettings
    {
        public bool Enabled { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public string CloudName { get; set; }

        public bool PrependCdnFolderName { get; set; }

        public string CdnFolderName { get; set; }

        public bool EnableJsCdn { get; set; }

        public bool EnableCssCdn { get; set; }

        public bool AutoGenerateRequiredPictures { get; set; }

        public bool AutoGenerateProductThumbPicture { get; set; }

        public bool AutoGenerateProductDetailsPicture { get; set; }

        public bool AutoGenerateProductThumbPictureOnProductDetailsPage { get; set; }

        public bool AutoGenerateAssociatedProductPicture { get; set; }

        public bool AutoGenerateCategoryThumbPicture { get; set; }

        public bool AutoGenerateManufacturerThumbPicture { get; set; }

        public bool AutoGenerateVendorThumbPicture { get; set; }

        public bool AutoGenerateCartThumbPicture { get; set; }

        public bool AutoGenerateMiniCartThumbPicture { get; set; }

        public bool AutoGenerateAutoCompleteSearchThumbPicture { get; set; }
    }
}