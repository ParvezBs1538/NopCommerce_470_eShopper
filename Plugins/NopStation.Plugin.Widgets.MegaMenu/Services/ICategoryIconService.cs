using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.MegaMenu.Domains;

namespace NopStation.Plugin.Widgets.MegaMenu.Services;

public interface ICategoryIconService
{
    Task DeleteCategoryIconAsync(CategoryIcon categoryIcon);

    Task InsertCategoryIconAsync(CategoryIcon categoryIcon);

    Task UpdateCategoryIconAsync(CategoryIcon categoryIcon);

    Task<CategoryIcon> GetCategoryIconByIdAsync(int categoryIconId);

    IList<CategoryIcon> GetCategoryIconByIds(int[] categoryIconIds);

    Task<CategoryIcon> GetCategoryIconByCategoryIdAsync(int categoryId);

    Task<IPagedList<CategoryIcon>> GetAllCategoryIconsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task DeleteCategoryIconsAsync(List<CategoryIcon> categoryIcons);
}