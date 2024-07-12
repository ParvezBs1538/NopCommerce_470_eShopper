using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public partial record DMSOtpVerificationRequestModel : BaseNopModel
    {
        public int ShipmentId { get; set; }

        public string ShipmentOtp { get; set; }
    }
}
