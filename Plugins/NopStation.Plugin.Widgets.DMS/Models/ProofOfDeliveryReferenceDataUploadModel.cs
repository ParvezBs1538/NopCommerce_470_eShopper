using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public partial record ProofOfDeliveryReferenceDataUploadModel : BaseNopModel
    {
        // signature / delivery photo / otp : enum int value
        public int ProofOfDeliveryTypeId { get; set; }

        // photo id or otp id
        public int ProofOfDeliveryReferenceId { get; set; }

        public int ShipmentId { get; set; }

    }
}
