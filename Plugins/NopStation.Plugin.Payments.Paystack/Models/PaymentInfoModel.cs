using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.Paystack.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string Description { get; set; }
    }
}