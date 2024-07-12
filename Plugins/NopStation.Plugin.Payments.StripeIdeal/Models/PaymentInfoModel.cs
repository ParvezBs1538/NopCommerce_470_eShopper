using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeIdeal.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}
