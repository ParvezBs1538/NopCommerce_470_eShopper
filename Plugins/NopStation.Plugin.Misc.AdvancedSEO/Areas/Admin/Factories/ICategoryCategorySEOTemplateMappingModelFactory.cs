using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public interface ICategoryCategorySEOTemplateMappingModelFactory
    {
        Task<CategoryCategorySEOTemplateMappingModel> PrepareCategoryCategorySEOTemplateMappingModelAsync(CategoryCategorySEOTemplateMappingModel model, CategoryCategorySEOTemplateMapping categorySEOTemplate,
            bool excludeProperties = false);

        Task<CategoryCategorySEOTemplateMappingSearchModel> PrepareCategoryCategorySEOTemplateMappingSearchModelAsync(
            CategoryCategorySEOTemplateMappingSearchModel searchModel, CategorySEOTemplate categorySEOTemplate);

        Task<CategoryCategorySEOTemplateMappingListModel> PrepareCategoryCategorySEOTemplateMappingListModelAsync(CategoryCategorySEOTemplateMappingSearchModel searchModel);

        #region CategoryToMap

        Task<CategoryToMapSearchModel> PrepareCategoryToMapSearchModelAsync(
            CategoryToMapSearchModel searchModel, CategorySEOTemplate categorySEOTemplate);

        Task<CategoryToMapModel> PrepareCategoryToMapModelAsync(Category category,
            bool excludeProperties = false);

        Task<CategoryToMapListModel> PrepareCategoryToMapListModelAsync(CategoryToMapSearchModel searchModel);

        #endregion

    }
}
