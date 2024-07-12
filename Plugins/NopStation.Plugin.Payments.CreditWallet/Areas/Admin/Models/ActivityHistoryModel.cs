using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models
{
    public record ActivityHistoryModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.WalletCustomer")]
        public int WalletCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.WalletCustomer")]
        public string WalletCustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.WalletCustomer")]
        public string WalletCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.ActivityType")]
        public int ActivityTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.ActivityType")]
        public string ActivityTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.PreviousTotalCreditUsed")]
        public decimal PreviousTotalCreditUsed { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.CurrentTotalCreditUsed")]
        public decimal CurrentTotalCreditUsed { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreatedByCustomer")]
        public int CreatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreatedByCustomer")]
        public string CreatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.Note")]
        public string Note { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreditUsed")]
        public decimal CreditUsed { get; set; }
    }
}
