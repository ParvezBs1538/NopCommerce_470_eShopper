using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record CatalogCommissionSearchModel : BaseSearchModel
    {
        public CatalogCommissionSearchModel()
        {
            ProductSearchModel = new ProductSearchModel();
            CategorySearchModel = new CategorySearchModel();
            ManufacturerSearchModel = new ManufacturerSearchModel();
        }

        public ProductSearchModel ProductSearchModel { get; set; }

        public CategorySearchModel CategorySearchModel { get; set; }

        public ManufacturerSearchModel ManufacturerSearchModel { get; set; }

        public SearchType SearchType { get; set; }
    }

    public enum SearchType
    {
        Category,
        Product,
        Manufacturer
    }
}
