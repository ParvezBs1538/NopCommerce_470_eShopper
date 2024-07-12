using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeWallet.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string OrderTotal { get; set; }

        public string Currency { get; set; }

        public string PaymentIntentId { get; set; }

        public string PaymentIntentStatus { get; set; }

        public string Country { get; set; }

        public string PublishableKey { get; set; }
    }
}