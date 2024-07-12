using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.WebApi.Domains
{
    public partial class ApiDevice : BaseEntity
    {
        public string DeviceToken { get; set; }

        public int DeviceTypeId { get; set; }

        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public string SubscriptionId { get; set; }

        public bool IsRegistered { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public DeviceType DeviceType
        {
            get => (DeviceType)DeviceTypeId;
            set => DeviceTypeId = (int)value;
        }
    }
}