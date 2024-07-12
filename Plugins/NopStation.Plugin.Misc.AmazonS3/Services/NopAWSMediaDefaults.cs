using Nop.Core.Caching;
namespace NopStation.Plugin.Misc.AmazonS3.Services
{
    /// <summary>
    /// Represents default values related to media services
    /// </summary>
    public static partial class NopAWSMediaDefaults
    {
        #region Caching defaults for AWS

        /// <summary>
        /// Gets a key to cache whether thumb exists
        /// </summary>
        /// <remarks>
        /// {0} : thumb file name
        /// </remarks>
        public static CacheKey ThumbExistsAwsCacheKey => new CacheKey("Nop.aws.thumb.exists-{0}", ThumbsExistsAwsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ThumbsExistsAwsPrefixCacheKey => "Nop.aws.thumb.exists";

        public static CacheKey PictureCacheRawCacheKey => new CacheKey("Nop.aws.pictureCacheRaw-{0}", PictureCacheRawPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string PictureCacheRawPrefixCacheKey => "Nop.aws.pictureCacheRaw";

        #endregion
    }
}