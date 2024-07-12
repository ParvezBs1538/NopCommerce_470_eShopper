using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Shipping.Redx.Models
{
    public class SendPercelRequestModel
    {
        public SendPercelRequestModel()
        {
            ParcelDetails = new List<ParcelDetails>();
        }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty("customer_phone")]
        public string CustomerPhone { get; set; }

        [JsonProperty("delivery_area")]
        public string DeliveryArea { get; set; }

        [JsonProperty("delivery_area_id")]
        public int DeliveryAreaId { get; set; }

        [JsonProperty("customer_address")]
        public string CustomerAddress { get; set; }

        [JsonProperty("merchant_invoice_id")]
        public string MerchantInvoiceId { get; set; }

        [JsonProperty("cash_collection_amount")]
        public string CashCollectionAmount { get; set; }

        [JsonProperty("parcel_weight")]
        public int ParcelWeight { get; set; }

        [JsonProperty("instruction")]
        public string Instruction { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("parcel_details_json")]
        public List<ParcelDetails> ParcelDetails { get; set; }
    }

    public class ParcelDetails
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }
}