using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.ExchangeRate.Abstract.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.AbstractExchangeRate.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }
    }
}
