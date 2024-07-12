using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Domains
{
    public partial class WebAppDevice : BaseEntity
    {
        public string PushEndpoint { get; set; }

        public string PushP256DH { get; set; }

        public string PushAuth { get; set; }

        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public string VapidPublicKey { get; set; }

        public string VapidPrivateKey { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
