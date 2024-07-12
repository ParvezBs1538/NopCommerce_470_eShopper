using System;
using Nop.Core.Configuration;

namespace NopStation.Plugin.CRM.Zoho
{
    public class ZohoCRMSettings : ISettings
    {
        public DateTime AccessTokenValidity { get; set; }

        public string RefreshToken { get; set; }

        public bool UseSandbox { get; set; }

        public int DataCenterId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool SyncShipment { get; set; }

        public string ShipmentModuleName { get; set; }

        public bool SyncShipmentItem { get; set; }

        public string ShipmentItemModuleName { get; set; }

        public string AccessToken { get; set; }

        public DateTime ExpireOnUtc { get; set; }
    }
}
