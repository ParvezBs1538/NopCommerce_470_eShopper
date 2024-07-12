using Nop.Core.Configuration;

namespace NopStation.Plugin.ExchangeRate.Abstract
{
    public class AbstractExchangeRateSettings: ISettings
    {
        public string ApiKey { get; set; }
    }
}