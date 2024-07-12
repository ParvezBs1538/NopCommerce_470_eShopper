using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record InvoicePaymentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.WalletCustomer")]
        public int WalletCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.WalletCustomer")]
        public string WalletCustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.WalletCustomer")]
        public string WalletCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.InvoiceReference")]
        public string InvoiceReference { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.Note")]
        public string Note { get; set; }

        [UIHint("Date")]
        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.PaymentDate")]
        public DateTime PaymentDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.PaymentDate")]
        public string PaymentDateStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.CreatedByCustomer")]
        public int CreatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.CreatedByCustomer")]
        public string CreatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.UpdatedByCustomer")]
        public int UpdatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.Fields.UpdatedByCustomer")]
        public string UpdatedByCustomer { get; set; }

        public bool FromCustomerPage { get; set; }

        public string CurrencyCode { get; set; }
    }
}
