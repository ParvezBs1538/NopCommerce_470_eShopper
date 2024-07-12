using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Responses
{
    public class PaymentValidationResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("tran_date")]
        public string TransanctionDate { get; set; }

        [JsonProperty("tran_id")]
        public Guid TransanctionId { get; set; }

        [JsonProperty("val_id")]
        public string ValueId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("discount_amount")]
        public string DiscountAmount { get; set; }

        [JsonProperty("discount_percentage")]
        public string DiscountPercentage { get; set; }

        [JsonProperty("discount_remarks")]
        public string DiscountRemarks { get; set; }

        [JsonProperty("store_amount")]
        public string StoreAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("bank_tran_id")]
        public string BankTransanctionId { get; set; }

        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("card_no")]
        public string CardNo { get; set; }

        [JsonProperty("card_issuer")]
        public string CardIssuer { get; set; }

        [JsonProperty("card_brand")]
        public string CardBrand { get; set; }

        [JsonProperty("card_issuer_country")]
        public string CardIssuerCountry { get; set; }

        [JsonProperty("card_issuer_country_code")]
        public string CardIssuerCountryCode { get; set; }

        [JsonProperty("currency_type")]
        public string CurrencyType { get; set; }

        [JsonProperty("currency_amount")]
        public decimal CurrencyAmount { get; set; }

        [JsonProperty("currency_rate")]
        public decimal CurrencyRate { get; set; }

        [JsonProperty("base_fair")]
        public string BaseFair { get; set; }

        [JsonProperty("value_a")]
        public string ValueA { get; set; }

        [JsonProperty("value_b")]
        public string ValueB { get; set; }

        [JsonProperty("value_c")]
        public string ValueC { get; set; }

        [JsonProperty("value_d")]
        public string ValueD { get; set; }

        [JsonProperty("emi_instalment")]
        public string EmiInstalment { get; set; }

        [JsonProperty("emi_amount")]
        public string EmiAmount { get; set; }

        [JsonProperty("emi_description")]
        public string EmiDescription { get; set; }

        [JsonProperty("emi_issuer")]
        public string EmiIssuer { get; set; }

        [JsonProperty("account_details")]
        public string AccountDetails { get; set; }

        [JsonProperty("risk_title")]
        public string RiskTitle { get; set; }

        [JsonProperty("risk_level")]
        public string RiskLevel { get; set; }

        [JsonProperty("APIConnect")]
        public string APIConnect { get; set; }

        [JsonProperty("validated_on")]
        public string ValidatedOn { get; set; }

        [JsonProperty("gw_version")]
        public string GatewayVersion { get; set; }

        [JsonProperty("token_key")]
        public string TokenKey { get; set; }
    }
}