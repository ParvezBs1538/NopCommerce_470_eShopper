using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop
{
    public record OCarouselSearchModel : BaseSearchModel
    {
        public OCarouselSearchModel()
        {
            AvailableDataSources = new List<SelectListItem>();
            AvailableWidgetZones = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableActiveOptions = new List<SelectListItem>();
            SearchWidgetZones = new List<int>() { 0 };
            SearchDataSources = new List<int>() { 0 };
        }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchWidgetZones")]
        public IList<int> SearchWidgetZones { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchDataSources")]
        public IList<int> SearchDataSources { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive")]
        public int SearchActiveId { get; set; }
        public int VendorId { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }
        public IList<SelectListItem> AvailableDataSources { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableActiveOptions { get; set; }
    }
}