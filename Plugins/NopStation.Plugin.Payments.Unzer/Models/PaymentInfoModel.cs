using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Unzer.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        #region Ctor

        public PaymentInfoModel()
        {
            StoredCards = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public string SelectedPaymentType { get; set; }

        public string ResourceID { get; set; }

        public string Errors { get; set; }

        public string ApiPublicKey { get; set; }

        public bool IsCardActive { get; set; }

        public bool IsPaypalActive { get; set; }

        public bool IsSofortActive { get; set; }

        public bool IsEpsActive { get; set; }

        public string EPSBIC { get; set; }

        [NopResourceDisplayName("NopStation.Unzer.Fields.SaveCard")]
        public bool SaveCard { get; set; }

        [NopResourceDisplayName("NopStation.Unzer.Fields.StoredCard")]
        public string StoredCardId { get; set; }

        public IList<SelectListItem> StoredCards { get; set; }

        #endregion
    }
}