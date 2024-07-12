using System;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Shipping.Redx
{
    public class RedxSettings : ISettings
    {
        public decimal FlatShippingCharge { get; set; }

        public string ApiAccessToken { get; set; }

        public string ParcelTrackUrl { get; set; }

        public string ShipmentEventsUrl { get; set; }

        public bool UseSandbox { get; set; }

        public string BaseUrl { get; set; }

        public string SandboxUrl { get; set; }

        public Uri GetBaseUri()
        {
            return new Uri(UseSandbox ? SandboxUrl : BaseUrl);
        }
    }
}