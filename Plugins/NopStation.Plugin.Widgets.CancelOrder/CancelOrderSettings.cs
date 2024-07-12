using Nop.Core.Configuration;
using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.CancelOrder
{
    public class CancelOrderSettings : ISettings
    {
        public CancelOrderSettings()
        {
            CancellableOrderStatuses = new List<int>();
            CancellablePaymentStatuses = new List<int>();
            CancellableShippingStatuses = new List<int>();
        }

        public string WidgetZone { get; set; }

        public List<int> CancellableOrderStatuses { get; set; }

        public List<int> CancellablePaymentStatuses { get; set; }

        public List<int> CancellableShippingStatuses { get; set; }
    }
}
