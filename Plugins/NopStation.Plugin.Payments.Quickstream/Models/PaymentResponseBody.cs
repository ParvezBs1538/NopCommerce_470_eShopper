using System;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class PaymentResponseBody
    {
        public string sourceCode { get; set; }
        public string receiptNumber { get; set; }
        public string communityCode { get; set; }
        public string supplierBusinessCode { get; set; }
        public string paymentReference { get; set; }
        public string customerReferenceNumber { get; set; }
        public string paymentAmount { get; set; }
        public string surchargeAmount { get; set; }
        public string cardScheme { get; set; }
        public string settlementDate { get; set; }
        public DateTime createdDateTime { get; set; }
        public string responseCode { get; set; }
        public string responseDescription { get; set; }
        public string summaryCode { get; set; }
        public bool successFlag { get; set; }
    }
}
