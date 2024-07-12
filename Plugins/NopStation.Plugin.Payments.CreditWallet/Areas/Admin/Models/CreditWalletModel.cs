namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public class CreditWalletModel
    {
        public CreditWalletModel()
        {
            WalletModel = new WalletModel();
            InvoicePaymentSearchModel = new InvoicePaymentSearchModel();
            ActivityHistorySearchModel = new ActivityHistorySearchModel();
        }

        public bool NewWallet { get; set; }

        public WalletModel WalletModel { get; set; }

        public InvoicePaymentSearchModel InvoicePaymentSearchModel { get; set; }

        public ActivityHistorySearchModel ActivityHistorySearchModel { get; set; }
    }
}
