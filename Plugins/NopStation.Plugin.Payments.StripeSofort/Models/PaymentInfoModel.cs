using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.StripeSofort.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}
