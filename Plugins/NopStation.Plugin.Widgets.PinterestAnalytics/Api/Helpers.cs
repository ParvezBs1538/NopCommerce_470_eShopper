//Contributor: https://www.codeproject.com/Articles/493455/Server-side-Google-Analytics-Transactions

using System;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Api
{
    public class Helpers
    {
        /// <summary>
        /// Converts a DateTime to a UNIX timestamp
        /// </summary>
        public static long ConvertToUnixTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from the Unix Epoch
            var span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();

            //return the total seconds (which is a UNIX timestamp)
            return (long)span.TotalSeconds;
        }
    }
}
