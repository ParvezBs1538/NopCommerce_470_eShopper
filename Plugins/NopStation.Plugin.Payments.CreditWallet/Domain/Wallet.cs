using Nop.Core;

namespace NopStation.Plugin.Payments.CreditWallet.Domain
{
    public class Wallet : BaseEntity
    {
        public int WalletCustomerId { get; set; }

        public bool Active { get; set; }

        public decimal CreditLimit { get; set; }

        public decimal CreditUsed { get; set; }

        public decimal AvailableCredit { get; set; }

        public bool AllowOverspend { get; set; }

        public int CurrencyId { get; set; }

        public decimal WarnUserForCreditBelow { get; set; }
    }
}
