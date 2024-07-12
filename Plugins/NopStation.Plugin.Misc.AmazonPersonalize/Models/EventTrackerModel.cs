using Amazon.PersonalizeEvents.Model;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Models
{
    public class EventTrackerModel
    {
        public PutEventsRequest PutEventsRequest { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public string EventType { get; set; }
    }
}