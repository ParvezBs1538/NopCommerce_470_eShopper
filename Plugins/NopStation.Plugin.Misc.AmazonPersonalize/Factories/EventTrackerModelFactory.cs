using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.PersonalizeEvents.Model;
using Nop.Core;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Factories
{
    public class EventTrackerModelFactory : IEventTrackerModelFactory
    {
        #region Fields

        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;
        private readonly IStoreContext _storeContext;

        #endregion Fields

        #region Ctor

        public EventTrackerModelFactory(AmazonPersonalizeSettings amazonPersonalizeSettings,
            IStoreContext storeContext)
        {
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
            _storeContext = storeContext;
        }

        #endregion Ctor

        #region Methods

        public Task<EventTrackerModel> PreparePutEventsRequest(int customerId, int productId, EventTypeEnum eventType)
        {
            //record event
            var eventRequest = new PutEventsRequest
            {
                TrackingId = _amazonPersonalizeSettings.EventTrackerId,
                UserId = customerId.ToString(),
                SessionId = Guid.NewGuid().ToString() //Session_Id
            };

            var anEvent = new Event
            {
                EventType = eventType.ToString(),
                ItemId = productId.ToString(),
                SentAt = DateTime.UtcNow
            };

            var events = new List<Event> { anEvent };
            eventRequest.EventList = events;

            var model = new EventTrackerModel
            {
                PutEventsRequest = eventRequest,
                UserId = customerId.ToString(),
                ProductId = productId.ToString(),
                EventType = eventType.ToString()
            };
            return Task.FromResult(model);
        }
        public Task<EventTrackerModel> PrepareMultiplePutEventsRequest(EventReportLine eventReportLine)
        {
            //record event
            var eventRequest = new PutEventsRequest
            {
                TrackingId = _amazonPersonalizeSettings.EventTrackerId,
                UserId = eventReportLine.UserId.ToString(),
                SessionId = Guid.NewGuid().ToString() //Session_Id
            };

            var purchaseEvent = new Event
            {
                EventType = EventTypeEnum.Purchase.ToString(),
                ItemId = eventReportLine.ItemId.ToString(),
                SentAt = eventReportLine.CreatedOnUtc
            };

            var viewEvent = new Event
            {
                EventType = EventTypeEnum.View.ToString(),
                ItemId = eventReportLine.ItemId.ToString(),
                SentAt = eventReportLine.CreatedOnUtc.AddMinutes(-1)
            };

            var events = new List<Event>();
            events.Add(purchaseEvent);
            events.Add(viewEvent);

            eventRequest.EventList = events;

            var model = new EventTrackerModel
            {
                PutEventsRequest = eventRequest,
                UserId = eventReportLine.UserId.ToString(),
                ProductId = eventReportLine.ItemId.ToString(),
                EventType = String.Empty
            };
            return Task.FromResult(model);
        }

        #endregion Methods
    }
}