using System;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class ElapsedPaymentHistory
    {
        public RecurringPaymentHistory History { get; set; }
        public DateTime ForDay { get; set; }
    }
}
