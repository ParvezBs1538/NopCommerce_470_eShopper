using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategorySEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public interface ICategorySEOTemplateModelFactory
    {
        Task<CategorySEOTemplateModel> PrepareCategorySEOTemplateModelAsync(CategorySEOTemplateModel model, CategorySEOTemplate categorySEOTemplate,
            bool excludeProperties = false);

        Task<CategorySEOTemplateSearchModel> PrepareCategorySEOTemplateSearchModelAsync(CategorySEOTemplateSearchModel searchModel);

        Task<CategorySEOTemplateListModel> PrepareCategorySEOTemplateListModelAsync(CategorySEOTemplateSearchModel searchModel);

    }
}
