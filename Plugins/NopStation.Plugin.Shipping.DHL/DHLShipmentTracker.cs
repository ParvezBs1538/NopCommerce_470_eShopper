using Nop.Services.Logging;
using NopStation.Plugin.Shipping.DHL.Domain;
using NopStation.Plugin.Shipping.DHL.Services;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Models;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Shipping.DHL
{
    public class DHLShipmentTracker : IShipmentTracker
    {
        private readonly ILogger _logger;
        private readonly IDHLShipmentService _dhlShipmentService;
        private readonly DHLSettings _dhlSettings;

        public DHLShipmentTracker(ILogger logger,
            IDHLShipmentService dhlShipmentService, 
            DHLSettings dhlSettings)
        {
            _logger = logger;
            _dhlShipmentService = dhlShipmentService;
            _dhlSettings = dhlSettings;
        }

        public virtual Task<bool> IsMatchAsync(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return Task.FromResult(false);

            //Not sure if this is true for every shipment, but it is true for all of our shipments
            return Task.FromResult(trackingNumber.Length == 10);
        }
        
        public virtual Task<string> GetUrlAsync(string trackingNumber, Shipment shipment)
        {
            var url = string.Empty;

            if (!string.IsNullOrEmpty(_dhlSettings.TrackingUrl))
                url = string.Concat(_dhlSettings.TrackingUrl, "?AWB=", trackingNumber, "&brand=DHL");

            return Task.FromResult(url);
        }

        private string CreateTrackingBody(DHLShipment dhlShipment)
        {
            var sb = new StringBuilder();

            sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
            sb.Append("<req:KnownTrackingRequest xmlns:req='http://www.dhl.com' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://www.dhl.com TrackingRequestKnown.xsd'>");
            sb.Append("<Request>");
            sb.Append("<ServiceHeader>");
            sb.AppendFormat("<MessageTime>{0}</MessageTime>", DateTime.Now.ToString("yyyy-MM-ddTHH:MM:ss"));
            sb.AppendFormat("<MessageReference>{0}</MessageReference>", dhlShipment.MessageReference);
            sb.AppendFormat("<SiteID>{0}</SiteID>", _dhlSettings.SiteId);
            sb.AppendFormat("<Password>{0}</Password>", _dhlSettings.Password);
            sb.Append("</ServiceHeader>");
            sb.Append("</Request>");

            sb.Append("<LanguageCode>en</LanguageCode>");
            sb.AppendFormat("<AWBNumber>{0}</AWBNumber>", dhlShipment.AirwayBillNumber);
            sb.Append("<LevelOfDetails>ALL_CHECK_POINTS</LevelOfDetails>");
            sb.Append("<PiecesEnabled>S</PiecesEnabled>");
            sb.Append("</req:KnownTrackingRequest>");

            return sb.ToString();
        }

        private async Task<DHLTrackingResponseModel> ParseTrackingRequestAsync(string responseXML)
        {
            var responseModel = new DHLTrackingResponseModel();

            using (var stringReader = new StringReader(responseXML))
            {
                using (var textReader = new XmlTextReader(stringReader))
                {
                    while (textReader.Read())
                    {
                        if (textReader.Name == "ActionStatus" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (textReader.Name == "Condition" && textReader.NodeType == XmlNodeType.Element)
                                {
                                    while (textReader.Read())
                                    {
                                        if (textReader.Name == "ConditionData" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            await _logger.ErrorAsync(textReader.ReadString());
                                            responseModel.Error = textReader.ReadString();
                                            return responseModel;
                                        }
                                    }
                                }
                            }
                        }

                        if (textReader.Name == "AWBInfo" && textReader.NodeType == XmlNodeType.Element)
                        {
                            while (textReader.Read())
                            {
                                if (textReader.Name == "ShipmentInfo" && textReader.NodeType == XmlNodeType.Element)
                                {
                                    while (textReader.Read())
                                    {
                                        if (textReader.Name == "ShipmentEvent" && textReader.NodeType == XmlNodeType.Element)
                                        {
                                            var shipmentStatusEvent = new ShipmentStatusEvent();
                                            var dateTime = string.Empty;
                                            while (textReader.Read())
                                            {
                                                if (textReader.Name == "Date" && textReader.NodeType == XmlNodeType.Element)
                                                {
                                                    dateTime = textReader.ReadString();
                                                }
                                                if (textReader.Name == "Time" && textReader.NodeType == XmlNodeType.Element)
                                                {
                                                    dateTime = dateTime + " " + textReader.ReadString();
                                                    shipmentStatusEvent.Date = DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                                }

                                                if (textReader.Name == "ServiceEvent" && textReader.NodeType == XmlNodeType.Element)
                                                {
                                                    while (textReader.Read())
                                                    {
                                                        if (textReader.Name == "Description" && textReader.NodeType == XmlNodeType.Element)
                                                        {
                                                            shipmentStatusEvent.EventName = textReader.ReadString();
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (textReader.Name == "Signatory" && textReader.NodeType == XmlNodeType.Element)
                                                {
                                                    shipmentStatusEvent.EventName += textReader.ReadString();
                                                }

                                                if (textReader.Name == "ServiceArea" && textReader.NodeType == XmlNodeType.Element)
                                                {
                                                    while (textReader.Read())
                                                    {
                                                        if (textReader.Name == "Description" && textReader.NodeType == XmlNodeType.Element)
                                                        {
                                                            shipmentStatusEvent.Location = textReader.ReadString();
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (shipmentStatusEvent.Date.HasValue && shipmentStatusEvent.EventName != null && shipmentStatusEvent.Location != null)
                                                {
                                                    responseModel.ShipmentStatusEvents.Add(shipmentStatusEvent);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return responseModel;
        }

        public async Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(string trackingNumber, Shipment shipment)
        {
            if (string.IsNullOrEmpty(trackingNumber))
                return new List<ShipmentStatusEvent>();

            var dHLShipment = _dhlShipmentService.GetDHLShipmentSubmissionByAirwayBillNumber(trackingNumber);

            if (dHLShipment == null)
                return new List<ShipmentStatusEvent>();

            if (string.IsNullOrEmpty(dHLShipment.AirwayBillNumber) && string.IsNullOrEmpty(dHLShipment.MessageReference))
                return new List<ShipmentStatusEvent>();

            var requestBody = CreateTrackingBody(dHLShipment);
            var responseXML = Utilities.DoRequest(_dhlSettings.Url, requestBody);

            var responseModel = await ParseTrackingRequestAsync(responseXML);

            if (string.IsNullOrEmpty(responseModel.Error))
            {
                return responseModel.ShipmentStatusEvents;
            }

            return new List<ShipmentStatusEvent>();
        }
    }
}
