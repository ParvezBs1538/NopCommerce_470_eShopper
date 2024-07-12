using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public partial record ProofOfDeliveryReferenceDataUploadResponseModel : BaseNopModel
    {
        public int ProofOfDeliveryDataId { get; set; }
    }
}
