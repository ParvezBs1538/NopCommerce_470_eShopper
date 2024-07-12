namespace Nopstation.Plugin.Widgets.DooFinder
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class DooFinderDefaults
    {
        /// <summary>
        /// Generic attribute name to hide config settings block on the plugin configuration page
        /// </summary>
        public static string HideConfigBlock = "DooFinderPage.HideConfigBlock";

        /// <summary>
        /// Generic attribute name to hide scheduler settings block on the plugin configuration page
        /// </summary>
        public static string HideSchedulerBlock = "DooFinderPage.HideSchedulerBlock";

        /// <summary>
        /// Generic attribute name to hide settings block on the plugin configuration page
        /// </summary>
        public static string HideSettingsBlock = "DooFinderPage.HideSettingsBlock";

        /// <summary>
        /// Name of the generate feed task
        /// </summary>
        public static string GenerateFeedTaskName => "DooFinder Generate Feed";

        /// <summary>
        /// Type of the generate feed task
        /// </summary>
        public static string GenerateFeedTask => "Nopstation.Plugin.Widgets.DooFinder.Services.GenerateFeedTask";

        /// <summary>
        /// Type of the generate feed task
        /// </summary>
        public static string PLUGIN_SYSTEM_NAME => "Nopstation.Plugin.Widgets.DooFinder";
        /// <summary>
        /// Default synchronization period in hours
        /// </summary>
        public static int DefaultGenerateFeedPeriod => 1;
    }
}
