using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models
{
    public record StoreLocationSearchModel : BaseSearchModel
    {
        public StoreLocationSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.List.SearchStoreName")]
        public string SearchStoreName { set; get; }

        [NopResourceDisplayName("Admin.NopStation.StoreLocator.StoreLocations.List.SearchStoreId")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }
    }
}
