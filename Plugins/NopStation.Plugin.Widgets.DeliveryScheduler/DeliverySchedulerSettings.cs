using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.DeliveryScheduler
{
    public class DeliverySchedulerSettings : ISettings
    {
        public bool EnableScheduling { get; set; }

        public int NumberOfDaysToDisplay { get; set; }

        public int DisplayDayOffset { get; set; }

        public bool ShowRemainingCapacity { get; set; }

        public string DateFormat { get; set; }
    }
}