using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Factories;

public interface ICategoryIconModelFactory
{
    Task<CategoryIconSearchModel> PrepareCategoryIconSearchModelAsync(CategoryIconSearchModel searchModel);

    Task<CategoryIconListModel> PrepareCategoryIconListModelAsync(CategoryIconSearchModel searchModel);

    Task<CategoryIconModel> PrepareCategoryIconModelAsync(CategoryIconModel model, Category category);
}