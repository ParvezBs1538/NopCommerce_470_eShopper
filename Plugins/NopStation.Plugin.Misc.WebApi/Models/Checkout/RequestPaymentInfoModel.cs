using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.WebApi.Models.Checkout
{
    public class RequestPaymentInfoModel
    {
        public RequestPaymentInfoModel()
        {
            CustomValues = new List<KeyValuePair<string, object>>();
        }

        public int StoreId { get; set; }

        public int CustomerId { get; set; }

        public Guid OrderGuid { get; set; }

        public DateTime? OrderGuidGeneratedOnUtc { get; set; }

        public decimal OrderTotal { get; set; }

        public string PaymentMethodSystemName { get; set; }

        #region Payment method specific properties 

        public string CreditCardType { get; set; }

        public string CreditCardName { get; set; }

        public string CreditCardNumber { get; set; }

        public int CreditCardExpireYear { get; set; }

        public int CreditCardExpireMonth { get; set; }

        public string CreditCardCvv2 { get; set; }

        #endregion

        #region Recurring payments

        public int InitialOrderId { get; set; }

        public int RecurringCycleLength { get; set; }

        public RecurringProductCyclePeriod RecurringCyclePeriod { get; set; }

        public int RecurringTotalCycles { get; set; }

        #endregion

        public List<KeyValuePair<string, object>> CustomValues { get; set; }
    }
}
