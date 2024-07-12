using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.EveryPay.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public int[] Installments { get; set; }

        public string Token { get; set; }

        public string Uuid { get; set; }

        public string ApiKey { get; set; }

        public bool IsSandbox { get; set; }

        public int Amount { get; set; }
    }
}