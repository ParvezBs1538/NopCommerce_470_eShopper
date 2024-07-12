using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories
{
    public interface IAjaxFilterModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<AjaxFilterParentCategoryModel> PrepareParentCategoryModelAsync(AjaxFilterParentCategory ajaxFilterCategory);

        Task<AjaxFilterParentCategorySearchModel> PrepareParentCategorySearchModelAsync(AjaxFilterParentCategorySearchModel searchModel);

        Task<CategoryListModel> PrepareParentCategoryListModelAsync(AjaxFilterParentCategorySearchModel searchModel);

        Task<AjaxFilterParentCategoryListModel> PrepareSelectedParentCategoryListModelAsync(AjaxFilterParentCategorySearchModel searchModel);
        Task<AjaxFilterParentCategoryListSearchModel> PrepareParentCategoryListSearchModelAsync(AjaxFilterParentCategorySearchModel searchModel);
    }
}
