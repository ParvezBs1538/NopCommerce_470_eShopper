using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public class OTPRecord : BaseEntity, ISoftDeletedEntity
    {
        public int CustomerId { get; set; }

        public int ShipmentId { get; set; }

        public int OrderId { get; set; }

        //public int StoreId { get; set; }

        //public string PhoneNumber { get; set; }

        public string AuthenticationCode { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        //public DateTime SentOnUtc { get; set; }

        public bool Verified { get; set; }

        public int? VerifiedByShipperId { get; set; }

        public DateTime? VerifiedOnUtc { get; set; }

        //public string? SmsProviderName { get; set; }

        //public string MessageId { get; set; }

        public bool Deleted { get; set; }
    }
}
