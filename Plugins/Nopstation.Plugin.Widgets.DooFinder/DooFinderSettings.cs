using System;
using Nop.Core.Configuration;

namespace Nopstation.Plugin.Widgets.DooFinder
{
    public class DooFinderSettings : ISettings
    {
        /// <summary>
        /// Product picture size
        /// </summary>
        public int ProductPictureSize { get; set; }

        /// <summary>
        /// A value indicating whether we should calculate prices considering promotions (tier prices, discounts, special prices, etc)
        /// </summary>
        public bool PricesConsiderPromotions { get; set; }

        /// <summary>
        /// Currency identifier for which feed file(s) will be generated
        /// </summary>
        public int CurrencyId { get; set; }


        /// <summary>
        /// Static file name of the feed
        /// </summary>
        public string StaticFileName { get; set; }

        /// <summary>
        /// Number of days for expiration date
        /// </summary>
        public int ExpirationNumberOfDays { get; set; }

        /// <summary>
        /// Set the Schedule Feed Generating Hour
        /// </summary>
        public int ScheduleFeedGeneratingHour { get; set; }

        /// <summary>
        /// Set the Schedule Feed Generating Minute
        /// </summary>
        public int ScheduleFeedGeneratingMinute { get; set; }

        /// <summary>
        /// storing last schedule execution start time
        /// </summary>
        public DateTime? ScheduleFeedLastExecutionStartTime { get; set; }

        /// <summary>
        /// storing last schedule execution end time
        /// </summary>
        public DateTime? ScheduleFeedLastExecutionEndTime { get; set; }

        public bool IsFeedGenerating { get; set; }

        public bool ScheduleFeedIsExecutedForToday { get; set; }

        public string ApiScript { get; set; }

        public bool AddAttributes { get; set; }

        public bool ActiveScript { get; set; }
    }
}