using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Models
{
    public record PaymentOptionModel : BaseNopEntityModel, IStoreMappingSupportedModel
    {
        public PaymentOptionModel()
        {
            SelectedStoreIds = new List<int>();
        }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.PaymentType")]
        public string PaymentType { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.Logo")]
        public string Logo { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.Brand")]
        public string Brand { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayName")]
        public string DisplayName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.ShortName")]
        public string ShortName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.PictureId")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.PaymentOptions.Fields.SelectedStoreIds")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}
