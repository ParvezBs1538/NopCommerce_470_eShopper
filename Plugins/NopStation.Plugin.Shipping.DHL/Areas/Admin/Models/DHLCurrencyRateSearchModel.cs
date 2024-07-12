using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public record DHLCurrencyRateSearchModel : BaseSearchModel
    {
        public DHLCurrencyRateSearchModel()
        {
            Configuration = new ConfigurationModel();
        }

        public ConfigurationModel Configuration { get; set; }
    }
}
