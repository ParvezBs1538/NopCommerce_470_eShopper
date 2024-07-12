using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record WalletModel : BaseNopEntityModel
    {
        public WalletModel()
        {
            AvailableCurrencies = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.WalletCustomer")]
        public int WalletCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.WalletCustomer")]
        public string WalletCustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.WalletCustomer")]
        public string WalletCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.CreditLimit")]
        public decimal CreditLimit { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.CreditUsed")]
        public decimal CreditUsed { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.AvailableCredit")]
        public decimal AvailableCredit { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.AllowOverspend")]
        public bool AllowOverspend { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.Currency")]
        public int CurrencyId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.Wallets.Fields.WarnUserForCreditBelow")]
        public decimal WarnUserForCreditBelow { get; set; }

        public string CurrencyCode { get; set; }

        public IList<SelectListItem> AvailableCurrencies { get; set; }
    }
}
