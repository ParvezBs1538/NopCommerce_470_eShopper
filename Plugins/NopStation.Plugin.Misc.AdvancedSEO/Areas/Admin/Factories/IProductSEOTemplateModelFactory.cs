using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public interface IProductSEOTemplateModelFactory
    {
        Task<ProductSEOTemplateModel> PrepareProductSEOTemplateModelAsync(ProductSEOTemplateModel model, ProductSEOTemplate productSEOTemplate,
            bool excludeProperties = false);

        Task<ProductSEOTemplateSearchModel> PrepareProductSEOTemplateSearchModelAsync(ProductSEOTemplateSearchModel searchModel);

        Task<ProductSEOTemplateListModel> PrepareProductSEOTemplateListModelAsync(ProductSEOTemplateSearchModel searchModel);

    }
}
