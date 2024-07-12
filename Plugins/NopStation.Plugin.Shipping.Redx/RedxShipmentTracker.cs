using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Shipping.Redx.Models;
using NopStation.Plugin.Shipping.Redx.Services;
using Nop.Services.Shipping.Tracking;
using Nop.Core.Domain.Shipping;
using System;

namespace NopStation.Plugin.Shipping.Redx
{
    public class RedxShipmentTracker : IShipmentTracker
    {
        #region Fields

        private readonly RedxSettings _redxSettings;
        private readonly IRedxShipmentService _redxShipmentService;

        #endregion Fields

        #region Ctor

        public RedxShipmentTracker(RedxSettings redxSettings,
            IRedxShipmentService redxShipmentService)
        {
            _redxSettings = redxSettings;
            _redxShipmentService = redxShipmentService;
        }

        #endregion ctor

        #region Methods

        public async Task<string> GetUrlAsync(string trackingNumber, Shipment shipment = null)
        {
            var redxShipment = await _redxShipmentService.GetRedxShipmentByShipmentIdAsync(shipment?.Id ?? 0);
            if (redxShipment != null && !string.IsNullOrWhiteSpace(redxShipment.TrackingId))
                return string.Concat(_redxSettings.ParcelTrackUrl, "?trackingId=", redxShipment.TrackingId);

            return null;
        }

        public async Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(string trackingNumber, Shipment shipment = null)
        {
            var redxShipment = await _redxShipmentService.GetRedxShipmentByShipmentIdAsync(shipment?.Id ?? 0);
            if (redxShipment == null)
                return new List<ShipmentStatusEvent>();

            trackingNumber = redxShipment.TrackingId;

            if (string.IsNullOrEmpty(trackingNumber))
                return new List<ShipmentStatusEvent>();

            var shipmentStatusEvents = new List<ShipmentStatusEvent>();

            var headers = new Dictionary<string, string>();
            headers.Add("API-ACCESS-TOKEN", "Bearer " + _redxSettings.ApiAccessToken);

            var uri = new Uri(_redxSettings.ShipmentEventsUrl).Concat(redxShipment.TrackingId);
            var response = uri.Get<TrackingResponseModel>(headers);

            if(!string.IsNullOrWhiteSpace(response.Model.Message))
                return shipmentStatusEvents;

            foreach (var status in response.Model.Tracking)
            {
                shipmentStatusEvents.Add(new ShipmentStatusEvent
                {
                    EventName = status.MessageEnglish,
                    Date = status.Time
                });
            }
            return shipmentStatusEvents;
        }

        #endregion Methods
    }
}