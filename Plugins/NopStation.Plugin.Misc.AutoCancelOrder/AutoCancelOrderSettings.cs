using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.AutoCancelOrder
{
    public class AutoCancelOrderSettings : ISettings
    {
        public AutoCancelOrderSettings()
        {
            ApplyOnOrderStatuses = new List<int>();
            ApplyOnShippingStatuses = new List<int>();
        }

        public bool EnablePlugin { get; set; }

        public List<int> ApplyOnOrderStatuses { get; set; }

        public List<int> ApplyOnShippingStatuses { get; set; }

        public string SerializedApplyOnPaymentMethods { get; set; }

        public bool SendNotificationToCustomer { get; set; }

        public IList<PaymentMethodOffset> ApplyOnPaymentMethods
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SerializedApplyOnPaymentMethods))
                    return new List<PaymentMethodOffset>();

                try
                {
                    return JsonConvert.DeserializeObject<List<PaymentMethodOffset>>(SerializedApplyOnPaymentMethods) ?? new List<PaymentMethodOffset>();
                }
                catch
                {
                    return new List<PaymentMethodOffset>();
                }
            }
            set 
            {
                SerializedApplyOnPaymentMethods = JsonConvert.SerializeObject(value);
            }
        }
    }
}
