using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.AmazonS3
{
    public class AmazonS3Settings : ISettings
    {
        public int RegionEndpointId { get; set; }

        public int CannedACLId { get; set; }

        public bool AWSS3Enable { get; set; }

        public string AWSS3AccessKeyId { get; set; }

        public string AWSS3SecretKey { get; set; }

        public string AWSS3BucketName { get; set; }

        public string AWSS3RootUrl { get; set; }

        public int ExpiresDays { get; set; }

        public bool EnableCdn { get; set; }

        public string CdnBaseUrl { get; set; }
    }
}
