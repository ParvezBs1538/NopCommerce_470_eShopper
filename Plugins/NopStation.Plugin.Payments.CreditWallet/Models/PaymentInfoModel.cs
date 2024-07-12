using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
        public string AvailableCredit { get; set; }
        public string OrderReference { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }
        public bool HaveWarningMsg { get; set; }
        public string WarningMsg { get; set; }
    }
}
