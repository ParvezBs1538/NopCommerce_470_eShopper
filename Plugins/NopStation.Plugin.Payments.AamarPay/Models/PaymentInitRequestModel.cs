using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.AamarPay.Models;

public class PaymentInitRequestModel
{
    [JsonProperty("store_id")]
    public string StoreId { get; set; }

    [JsonProperty("tran_id")]
    public string TransactionId { get; set; }

    [JsonProperty("success_url")]
    public string SuccessUrl { get; set; }

    [JsonProperty("fail_url")]
    public string FailUrl { get; set; }

    [JsonProperty("cancel_url")]
    public string CancelUrl { get; set; }

    [JsonProperty("amount")]
    public string Amount { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("signature_key")]
    public string SignatureKey { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    [JsonProperty("cus_name")]
    public string CustomerName { get; set; }

    [JsonProperty("cus_email")]
    public string CustomerEmail { get; set; }

    [JsonProperty("cus_add1")]
    public string CustomerAddress1 { get; set; }

    [JsonProperty("cus_add2")]
    public string CustomerAddress2 { get; set; }

    [JsonProperty("cus_city")]
    public string CustomerCity { get; set; }

    [JsonProperty("cus_state")]
    public string CustomerState { get; set; }

    [JsonProperty("cus_postcode")]
    public string CustomerPostcode { get; set; }

    [JsonProperty("cus_country")]
    public string CustomerCountry { get; set; }

    [JsonProperty("cus_phone")]
    public string CustomerPhone { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}
