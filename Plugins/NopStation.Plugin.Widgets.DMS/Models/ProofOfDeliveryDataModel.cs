using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record ProofOfDeliveryDataModel : BaseNopEntityModel
    {
        public int CourierShipmentId { get; set; }

        public int NopShipmentId { get; set; }

        public int VerifiedByShipperId { get; set; }

        public string VerifiedByShipperEmail { get; set; }

        public string ProofOfDeliveryType { get; set; }

        public bool PODContainPhoto { get; set; }

        public string PODPhotoUrl { get; set; }

        public DateTime VerifiedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
