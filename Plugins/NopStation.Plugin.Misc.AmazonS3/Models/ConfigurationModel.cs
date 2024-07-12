using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AmazonS3.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3Enable")]
        public bool AWSS3Enable { get; set; }
        public bool AWSS3Enable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.CannedACL")]
        public int CannedACLId { get; set; }
        public bool CannedACLId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.RegionEndpoint")]
        public int RegionEndpointId { get; set; }
        public bool RegionEndpointId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3AccessKeyId")]
        public string AWSS3AccessKeyId { get; set; }
        public bool AWSS3AccessKeyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3SecretKey")]
        public string AWSS3SecretKey { get; set; }
        public bool AWSS3SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3BucketName")]
        public string AWSS3BucketName { get; set; }
        public bool AWSS3BucketName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3RootUrl")]
        public string AWSS3RootUrl { get; set; }
        public bool AWSS3RootUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.ExpiresDays")]
        public int ExpiresDays { get; set; }
        public bool ExpiresDays_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.EnableCdn")]
        public bool EnableCdn { get; set; }
        public bool EnableCdn_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.CdnBaseUrl")]
        public string CdnBaseUrl { get; set; }
        public bool CdnBaseUrl_OverrideForStore { get; set; }
    }
}
