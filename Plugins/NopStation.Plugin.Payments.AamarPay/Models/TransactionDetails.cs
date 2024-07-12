using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.AamarPay.Models;

public class TransactionDetails
{
    [JsonProperty("pg_txnid")]
    public string PgTransactionId { get; set; }

    [JsonProperty("mer_txnid")]
    public string MerchantTransactionId { get; set; }

    [JsonProperty("risk_title")]
    public string RiskTitle { get; set; }

    [JsonProperty("risk_level")]
    public string RiskLevel { get; set; }

    [JsonProperty("cus_name")]
    public string CustomerName { get; set; }

    [JsonProperty("cus_email")]
    public string CustomerEmail { get; set; }

    [JsonProperty("cus_phone")]
    public string CustomerPhone { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

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

    [JsonProperty("cus_fax")]
    public string CustomerFax { get; set; }

    [JsonProperty("ship_name")]
    public string ShipName { get; set; }

    [JsonProperty("ship_add1")]
    public string ShipAddress1 { get; set; }

    [JsonProperty("ship_add2")]
    public string ShipAddress2 { get; set; }

    [JsonProperty("ship_city")]
    public string ShipCity { get; set; }

    [JsonProperty("ship_state")]
    public string ShipState { get; set; }

    [JsonProperty("ship_postcode")]
    public string ShipPostcode { get; set; }

    [JsonProperty("ship_country")]
    public string ShipCountry { get; set; }

    [JsonProperty("merchant_id")]
    public string MerchantId { get; set; }

    [JsonProperty("store_id")]
    public string StoreId { get; set; }

    [JsonProperty("amount")]
    public string Amount { get; set; }

    [JsonProperty("amount_bdt")]
    public string AmountBdt { get; set; }

    [JsonProperty("pay_status")]
    public string PaymentStatus { get; set; }

    [JsonProperty("status_code")]
    public string StatusCode { get; set; }

    [JsonProperty("status_title")]
    public string StatusTitle { get; set; }

    [JsonProperty("cardnumber")]
    public string CardNumber { get; set; }

    [JsonProperty("approval_code")]
    public string ApprovalCode { get; set; }

    [JsonProperty("payment_processor")]
    public string PaymentProcessor { get; set; }

    [JsonProperty("bank_trxid")]
    public string BankTransactionId { get; set; }

    [JsonProperty("payment_type")]
    public string PaymentType { get; set; }

    [JsonProperty("error_code")]
    public string ErrorCode { get; set; }

    [JsonProperty("error_title")]
    public string ErrorTitle { get; set; }

    [JsonProperty("bin_country")]
    public string BinCountry { get; set; }

    [JsonProperty("bin_issuer")]
    public string BinIssuer { get; set; }

    [JsonProperty("bin_cardtype")]
    public string BinCardType { get; set; }

    [JsonProperty("bin_cardcategory")]
    public string BinCardCategory { get; set; }

    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("date_processed")]
    public string DateProcessed { get; set; }

    [JsonProperty("amount_currency")]
    public string AmountCurrency { get; set; }

    [JsonProperty("rec_amount")]
    public string ReceivedAmount { get; set; }

    [JsonProperty("processing_ratio")]
    public string ProcessingRatio { get; set; }

    [JsonProperty("processing_charge")]
    public string ProcessingCharge { get; set; }

    [JsonProperty("ip")]
    public string IpAddress { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("currency_merchant")]
    public string CurrencyMerchant { get; set; }

    [JsonProperty("convertion_rate")]
    public string ConversionRate { get; set; }

    [JsonProperty("opt_a")]
    public string OptionA { get; set; }

    [JsonProperty("opt_b")]
    public string OptionB { get; set; }

    [JsonProperty("opt_c")]
    public string OptionC { get; set; }

    [JsonProperty("opt_d")]
    public string OptionD { get; set; }

    [JsonProperty("verify_status")]
    public string VerifyStatus { get; set; }

    [JsonProperty("call_type")]
    public string CallType { get; set; }

    [JsonProperty("email_send")]
    public string EmailSend { get; set; }

    [JsonProperty("doc_recived")]
    public string DocumentReceived { get; set; }

    [JsonProperty("checkout_status")]
    public string CheckoutStatus { get; set; }
}
