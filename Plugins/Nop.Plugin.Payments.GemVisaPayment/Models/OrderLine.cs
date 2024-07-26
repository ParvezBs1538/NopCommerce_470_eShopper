using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class OrderLine
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("productUrl")]
        public string ProductUrl { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unitPrice")]
        public double UnitPrice { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("requiresShipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("isGiftCard")]
        public bool IsGiftCard { get; set; }
    }
}
