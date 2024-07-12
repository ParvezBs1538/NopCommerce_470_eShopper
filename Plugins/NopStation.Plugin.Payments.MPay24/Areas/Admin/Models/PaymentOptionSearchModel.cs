using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Models
{
    public record PaymentOptionSearchModel : BaseSearchModel
    {
        public PaymentOptionSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.List.SearchStoreId")]
        public virtual int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.List.SearchName")]
        public virtual string SearchName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.List.SearchBrand")]
        public virtual string SearchBrand { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.List.SearchPaymentType")]
        public virtual string SearchPaymentType { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}
