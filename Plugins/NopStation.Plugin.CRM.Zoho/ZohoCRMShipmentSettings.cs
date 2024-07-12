using Nop.Core.Configuration;

namespace NopStation.Plugin.CRM.Zoho
{
    public class ZohoCRMShipmentSettings : ISettings
    {
        public string OrderField { get; set; }

        public string TrackingNumberField { get; set; }

        public string WeightField { get; set; }

        public string ShippedDateUtcField { get; set; }

        public string DeliveryDateUtcField { get; set; }

        public string ItemShipmentField { get; set; }

        public string ItemProductField { get; set; }

        public string ItemQuantityField { get; set; }
    }
}
