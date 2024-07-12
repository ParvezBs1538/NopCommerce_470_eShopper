using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.CopyAndPay.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
        public string Error { get; set; }
    }
}
