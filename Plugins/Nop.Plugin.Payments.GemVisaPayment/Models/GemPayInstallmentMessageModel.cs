using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class GemPayInstallmentMessageModel : BaseNopModel
    {
        public string Url { get; set; }
        public string MarchantId { get; set; }
        public string Currency { get; set; }
        public string PromotionMonths { get; set; }
        public decimal Price { get; set; }
    }
}
