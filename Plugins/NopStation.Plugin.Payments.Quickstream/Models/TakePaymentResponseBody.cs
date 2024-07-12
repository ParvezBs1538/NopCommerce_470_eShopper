using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class Amount
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("amount")]
        public double TotalAmount { get; set; }
        [JsonProperty("displayAmount")]
        public string DisplayAmount { get; set; }
    }

    public class MerchantAccount
    {
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }


        [JsonProperty("merchantName")]
        public string MerchantName { get; set; }

        [JsonProperty("settlementBsb")]
        public string SettlementBsb { get; set; }

        [JsonProperty("settlementAccountNumber")]
        public string SettlementAccountNumber { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("acquiringInstitution")]
        public string AcquiringInstitution { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("extraInformation")]
        public string ExtraInformation { get; set; }


        [JsonProperty("moreInformation")]
        public string MoreInformation { get; set; }
    }

    public class TakePaymentResponseBody
    {
        [JsonProperty("receiptNumber")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseDescription")]
        public string ResponseDescription { get; set; }

        [JsonProperty("summaryCode")]
        public string SummaryCode { get; set; }

        [JsonProperty("fraudGuardResult")]
        public object FraudGuardResult { get; set; }

        [JsonProperty("customerReferenceNumber")]
        public string CustomerReferenceNumber { get; set; }

        [JsonProperty("paymentReferenceNumber")]
        public string PaymentReferenceNumber { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("settlementDate")]
        public string SettlementDate { get; set; }

        [JsonProperty("transactionTime")]
        public DateTime TransactionTime { get; set; }

        [JsonProperty("refundable")]
        public bool Refundable { get; set; }

        [JsonProperty("voidable")]
        public bool Voidable { get; set; }

        [JsonProperty("principalAmount")]
        public Amount PrincipalAmount { get; set; }

        [JsonProperty("surchargeAmount")]
        public Amount SurchargeAmount { get; set; }

        [JsonProperty("totalAmount")]
        public Amount TotalAmount { get; set; }

        [JsonProperty("merchantAccount")]
        public MerchantAccount MerchantAccount { get; set; }

        [JsonProperty("creditCard")]
        public CreditCard CreditCard { get; set; }

        [JsonProperty("bankAccount")]
        public BankAccount BankAccountInfo { get; set; }

        [JsonProperty("directEntryAccount")]
        public DirectEntryAccount DirectEntryAccountInfo { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("links")]
        public List<Link> Links { get; set; }
    }

    public class BankAccount
    {
        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("accountToken")]
        public object AccountToken { get; set; }

        [JsonProperty("customerId")]
        public object CustomerId { get; set; }

        [JsonProperty("defaultAccount")]
        public bool DefaultAccount { get; set; }

        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("bsb")]
        public string Bsb { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }
    }

    public class DirectEntryAccount
    {
        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("directEntryUserId")]
        public string DirectEntryUserId { get; set; }

        [JsonProperty("directEntryUserName")]
        public string DirectEntryUserName { get; set; }

        [JsonProperty("settlementBsb")]
        public string SettlementBsb { get; set; }

        [JsonProperty("settlementAccountNumber")]
        public string SettlementAccountNumber { get; set; }
    }
}
