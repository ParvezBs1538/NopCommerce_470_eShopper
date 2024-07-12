using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface ICategoryIconService
    {
        Task DeleteCategoryIconAsync(ApiCategoryIcon categoryIcon);

        Task InsertCategoryIconAsync(ApiCategoryIcon categoryIcon);

        Task UpdateCategoryIconAsync(ApiCategoryIcon categoryIcon);

        Task<ApiCategoryIcon> GetCategoryIconByIdAsync(int categoryIconId);

        Task<IList<ApiCategoryIcon>> GetCategoryIconByIdsAsync(int[] categoryIconIds);

        Task<ApiCategoryIcon> GetCategoryIconByCategoryIdAsync(int categoryId);

        Task<IPagedList<ApiCategoryIcon>> GetAllCategoryIconsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task DeleteCategoryIconsAsync(List<ApiCategoryIcon> categoryIcons);
    }
}