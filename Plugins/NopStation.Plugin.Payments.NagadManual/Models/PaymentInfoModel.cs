using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.NagadManual.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }

        public string PhoneNumber { get; set; }

        public string TransactionId { get; set; }
    }
}