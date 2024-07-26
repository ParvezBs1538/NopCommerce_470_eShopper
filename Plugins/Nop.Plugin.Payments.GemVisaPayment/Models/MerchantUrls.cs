using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class MerchantUrls
    {
        [JsonProperty("cancel")]
        public string Cancel { get; set; }

        [JsonProperty("complete")]
        public string Complete { get; set; }
    }

}
