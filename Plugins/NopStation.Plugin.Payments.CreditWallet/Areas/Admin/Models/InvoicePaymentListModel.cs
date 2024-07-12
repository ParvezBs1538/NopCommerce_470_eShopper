using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record InvoicePaymentListModel : BasePagedListModel<InvoicePaymentModel>
    {
    }
}
