using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Models
{
    public record WalletDetailsModel : BaseNopEntityModel
    {
        public WalletDetailsModel()
        {
            Invoices = new List<InvoicePaymentModel>();
        }

        public bool Active { get; set; }

        public decimal CreditLimit { get; set; }

        public decimal CreditUsedValue { get; set; }

        public string CreditUsed { get; set; }

        public decimal AvailableCreditValue { get; set; }

        public string AvailableCredit { get; set; }

        public bool AllowOverspend { get; set; }

        public bool LowCredit { get; set; }

        public bool ShowInvoicesInCustomerWalletPage { get; set; }

        public IList<InvoicePaymentModel> Invoices { get; set; }


        public record InvoicePaymentModel : BaseNopEntityModel
        {
            public string InvoiceReference { get; set; }

            public DateTime PaymentDate { get; set; }

            public string PaymentDateStr { get; set; }

            public decimal AmountValue { get; set; }

            public string Amount { get; set; }

            public string Note { get; set; }
        }
    }
}
