using System.Threading.Tasks;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Factories
{
    public interface IEventTrackerModelFactory
    {
        Task<EventTrackerModel> PreparePutEventsRequest(int customerId, int productId, EventTypeEnum eventType);
        Task<EventTrackerModel> PrepareMultiplePutEventsRequest(EventReportLine eventReportLine);
    }
} 