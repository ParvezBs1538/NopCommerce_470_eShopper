using System;

namespace NopStation.Plugin.Payments.POLiPay.Models
{
    public class PaymentStatusResponse
    {
        public string CountryName { get; set; }
        public string FinancialInstitutionCountryCode { get; set; }
        public string TransactionID { get; set; }
        public DateTime MerchantEstablishedDateTime { get; set; }
        public string PayerAccountNumber { get; set; }
        public string PayerAccountSortCode { get; set; }
        public string MerchantAccountSortCode { get; set; }
        public string MerchantAccountName { get; set; }
        public string MerchantData { get; set; }
        public string CurrencyName { get; set; }
        public string TransactionStatus { get; set; }
        public bool IsExpired { get; set; }
        public string MerchantEntityID { get; set; }
        public string UserIPAddress { get; set; }
        public string POLiVersionCode { get; set; }
        public string MerchantName { get; set; }
        public string TransactionRefNo { get; set; }
        public string CurrencyCode { get; set; }
        public string CountryCode { get; set; }
        public double PaymentAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime EstablishedDateTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string BankReceipt { get; set; }
        public string BankReceiptDateTime { get; set; }
        public string TransactionStatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string FinancialInstitutionCode { get; set; }
        public string FinancialInstitutionName { get; set; }
        public string MerchantReference { get; set; }
        public object MerchantAccountSuffix { get; set; }
        public string MerchantAccountNumber { get; set; }
        public string PayerFirstName { get; set; }
        public string PayerFamilyName { get; set; }
        public string PayerAccountSuffix { get; set; }
    }
}
