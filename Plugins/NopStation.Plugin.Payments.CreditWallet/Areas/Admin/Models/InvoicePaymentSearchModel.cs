using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record InvoicePaymentSearchModel : BaseSearchModel
    {
        public int SearchWalletCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.List.SearchInvoiceReference")]
        public string SearchInvoiceReference { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.List.SearchCreatedFrom")]
        public DateTime? SearchCreatedFrom { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.NopStation.CreditWallet.InvoicePayments.List.SearchCreatedTo")]
        public DateTime? SearchCreatedTo { get; set; }
    }
}
