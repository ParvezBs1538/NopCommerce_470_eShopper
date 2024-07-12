using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.WebApi.Models.Catalog;

namespace NopStation.Plugin.Misc.WebApi.Factories
{
    public interface ICatalogApiModelFactory
    {
        Task<IList<CategoryTreeModel>> PrepareCategoryTreeModelAsync();

        Task<IList<HomepageCategoryModel>> PrepareHomepageCategoriesWithProductsModelAsync();

        Task<IList<ManufacturerModel>> PrepareHomepageManufacturerModelsAsync();
    }
}
