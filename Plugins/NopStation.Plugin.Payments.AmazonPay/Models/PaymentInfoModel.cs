using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.AmazonPay.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}