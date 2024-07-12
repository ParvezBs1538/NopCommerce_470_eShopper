using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services
{
    public interface IAjaxFilterParentCategoryService
    {
        Task<AjaxFilterParentCategory> GetParentCategoryByIdAsync(int id);
        Task<AjaxFilterParentCategory> GetParentCategoryByCategoryIdAsync(int id);
        Task InsertParentCategoryAsync(AjaxFilterParentCategory ajaxFilterParentCategory);
        Task UpdateParentCategoryAsync(AjaxFilterParentCategory ajaxFilterParentCategory);
        Task DeleteParentCategoryAsync(AjaxFilterParentCategory ajaxFilterParentCategory);
        Task<IPagedList<Category>> GetParentCategoriesAsync(AjaxFilterParentCategorySearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<AjaxFilterParentCategory>> GetSelectedParentCategoriesAsync(AjaxFilterParentCategorySearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<bool> CanOverrideFilterSetAsync(int categoryId);
    }
}
