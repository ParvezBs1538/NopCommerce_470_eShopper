using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Tax.TaxJar.Models
{
    public record TaxJarTransactionLogModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.TransactionId")]
        public string TransactionId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.TransactionReferance")]
        public string TransactionReferanceId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.TransactionType")]
        public string TransactionType { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.User")]
        public int UserId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.Order")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.Customer")]
        public int CustomerId { get; set; }
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Log.TransactionDate")]
        public DateTime TransactionDate { get; set; }

        #endregion
    }
}
