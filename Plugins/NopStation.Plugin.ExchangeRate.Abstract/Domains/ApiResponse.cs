using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.ExchangeRate.Abstract.Domains
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            ExchangeRates = new Dictionary<string, decimal>();
            Error = new ErrorData();
        }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("exchange_rates")]
        public IDictionary<string, decimal> ExchangeRates { get; set; }

        [JsonProperty("error")]
        public ErrorData Error { get; set; }

        public class ErrorDetails
        {
            [JsonProperty("target")]
            public List<string> Target { get; set; }
        }

        public class ErrorData
        {
            public ErrorData()
            {
                Details = new ErrorDetails();
            }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("details")]
            public ErrorDetails Details { get; set; }
        }
    }
}
