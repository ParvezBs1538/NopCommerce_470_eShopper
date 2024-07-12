using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Factories
{
    public interface ICategoryBannerModelFactory
    {
        Task<CategoryBannerListModel> PrepareProductPictureListModelAsync(CategoryBannerSearchModel searchModel, Category category);
    }
}