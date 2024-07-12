using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public interface ICategoryIconModelFactory
    {
        Task<CategoryIconSearchModel> PrepareCategoryIconSearchModelAsync(CategoryIconSearchModel searchModel);

        Task<CategoryIconListModel> PrepareCategoryIconListModelAsync(CategoryIconSearchModel searchModel);

        Task<CategoryIconModel> PrepareCategoryIconModelAsync(CategoryIconModel model, Category category);
    }
}