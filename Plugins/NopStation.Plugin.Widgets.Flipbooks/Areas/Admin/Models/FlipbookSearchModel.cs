using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    public record FlipbookSearchModel : BaseSearchModel
    {
        public FlipbookSearchModel()
        {
            AvailableActiveOptions = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.List.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive")]
        public int SearchActiveId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public bool HideStoresList { get; set; }

        public IList<SelectListItem> AvailableActiveOptions { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}
