using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AjaxFilter.Domains;
using NopStation.Plugin.Misc.AjaxFilter.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Services
{
    public interface IAjaxFilterService
    {
        Task<int> GetNumberOfProductsInManufacturerAsync(int categoryId, IList<int> manufacturerIds = null, int storeId = 0);
        Task<int> GetNumberOfProductsUsingSpecificationAttributeAsync(int categoryId, int specificationAttributeOptionId, int storeId = 0);

        Task<SearchFilterResult> SearchProducts(SearchModel model, string typ = "", bool showproducts = false);

        Task<List<AjaxFilterSpecificationAttribute>> GetAllAjaxFilterSpecificationAttributeIdsFromCategoryId(int categoryId);

        Task<int> GetSpecificationAttributeOptionsCountBySpecificationId(int id);
    }
}