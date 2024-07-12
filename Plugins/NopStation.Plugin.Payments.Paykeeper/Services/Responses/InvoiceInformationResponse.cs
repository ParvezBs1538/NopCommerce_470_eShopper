using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paykeeper.Services.Responses
{
    public class InvoiceInformationResponse
    {
        [JsonProperty("invoice_id")]
        public string InvoiceId { get; set; }

        [JsonProperty("invoice_url")]
        public string InvoiceUrl { get; set; }

        [JsonProperty("invoice")]
        public string Invoice { get; set; }
    }
}
