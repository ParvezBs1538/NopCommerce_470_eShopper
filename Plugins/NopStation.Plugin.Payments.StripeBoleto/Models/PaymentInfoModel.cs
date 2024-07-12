using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeBoleto.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string TaxID { get; set; }
    }
}
