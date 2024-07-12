using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.Instamojo.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public string Description { get; set; }
    }
}