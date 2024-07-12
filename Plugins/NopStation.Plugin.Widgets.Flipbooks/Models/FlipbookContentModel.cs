using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.Flipbooks.Models
{
    public record FlipbookContentModel : BaseNopEntityModel
    {
        public FlipbookContentModel()
        {
            ProductOverviewModelList = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogProductsCommand();
        }

        public string ImageUrl { get; set; }

        public string RedirectUrl { get; set; }

        public bool IsImage { get; set; }

        public int PageNumber { get; set; }

        public IList<ProductOverviewModel> ProductOverviewModelList { get; set; }

        public CatalogProductsCommand PagingFilteringContext { get; set; }
    }
}
