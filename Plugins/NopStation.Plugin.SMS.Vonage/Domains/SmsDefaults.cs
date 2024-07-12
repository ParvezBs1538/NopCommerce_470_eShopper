namespace NopStation.Plugin.SMS.Vonage.Domains
{
    /// <summary>
    /// Represents default values related to messages services
    /// </summary>
    public static partial class SmsDefaults
    {
        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static string MessageTemplatesAllCacheKey => "Nop.VonageSms.SmsTemplate.all-{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        public static string MessageTemplatesByNameCacheKey => "Nop.VonageSms.SmsTemplate.name-{0}-{1}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string MessageTemplatesPrefixCacheKey => "Nop.VonageSms.SmsTemplate.";
    }
}