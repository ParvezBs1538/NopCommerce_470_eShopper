using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public partial class ShipperDevice : BaseEntity
    {
        public string DeviceToken { get; set; }

        public int DeviceTypeId { get; set; }

        public int CustomerId { get; set; }
        public bool Online { get; set; }

        //public int StoreId { get; set; }

        public string SubscriptionId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        //public bool IsRegistered { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public DateTime? LocationUpdatedOnUtc { get; set; }

        public DeviceType DeviceType
        {
            get => (DeviceType)DeviceTypeId;
            set => DeviceTypeId = (int)value;
        }
    }
}
