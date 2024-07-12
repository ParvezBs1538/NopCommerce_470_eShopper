using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.PayHere.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string Description { get; set; }
    }
}