using Nop.Core;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Domains
{
    public partial class Recommendation : BaseEntity
    {
        public string Title { get; set; }
        public bool Active { get; set; }
        public int NumberOfItemsToShow { get; set; }
        public string RecommenderARN { get; set; }
        public int WidgetZoneId { get; set; }
        public bool AllowForGuestCustomer { get; set; }
    }
}