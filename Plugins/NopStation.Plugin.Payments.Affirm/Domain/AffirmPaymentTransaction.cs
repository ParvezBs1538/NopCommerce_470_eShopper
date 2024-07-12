using System;
using Nop.Core;

namespace NopStation.Plugin.Payments.Affirm.Domain
{
    public class AffirmPaymentTransaction : BaseEntity
    {
        public string ChargeId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string Currency { get; set; }

        public int Amount { get; set; }

        public int AuthHold { get; set; }

        public int Payable { get; set; }

        public bool IsVoid { get; set; }

        public DateTime ExpiredOnUtc { get; set; }

        public Guid OrderGuid { get; set; }
    }
}
