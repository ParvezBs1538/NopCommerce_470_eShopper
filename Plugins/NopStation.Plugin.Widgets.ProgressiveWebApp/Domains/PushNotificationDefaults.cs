namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Domains
{
    /// <summary>
    /// Represents default values related to messages services
    /// </summary>
    public static partial class PushNotificationDefaults
    {
        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static string MessageTemplatesAllCacheKey => "Nop.pushnotificationtemplate.all-{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        public static string MessageTemplatesByNameCacheKey => "Nop.pushnotificationtemplate.name-{0}-{1}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string MessageTemplatesPrefixCacheKey => "Nop.pushnotificationtemplate.";
    }
}