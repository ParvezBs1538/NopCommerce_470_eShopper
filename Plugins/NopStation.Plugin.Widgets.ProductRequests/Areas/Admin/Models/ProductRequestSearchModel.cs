using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models
{
    public record ProductRequestSearchModel : BaseSearchModel
    {
        public ProductRequestSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.List.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.List.SearchCustomerEmail")]
        public string SearchCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}