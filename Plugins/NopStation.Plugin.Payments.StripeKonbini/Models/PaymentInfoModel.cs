using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeKonbini.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string ConfirmationNumber { get; set; }
    }
}
