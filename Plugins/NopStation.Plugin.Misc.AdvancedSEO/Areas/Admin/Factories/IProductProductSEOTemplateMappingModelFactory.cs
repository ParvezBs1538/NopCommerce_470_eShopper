using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public interface IProductProductSEOTemplateMappingModelFactory
    {
        Task<ProductProductSEOTemplateMappingModel> PrepareProductProductSEOTemplateMappingModelAsync(ProductProductSEOTemplateMappingModel model, ProductProductSEOTemplateMapping productSEOTemplate,
            bool excludeProperties = false);

        Task<ProductProductSEOTemplateMappingSearchModel> PrepareProductProductSEOTemplateMappingSearchModelAsync(
            ProductProductSEOTemplateMappingSearchModel searchModel, ProductSEOTemplate productSEOTemplate);

        Task<ProductProductSEOTemplateMappingListModel> PrepareProductProductSEOTemplateMappingListModelAsync(ProductProductSEOTemplateMappingSearchModel searchModel);

        #region ProductToMap

        Task<ProductToMapSearchModel> PrepareProductToMapSearchModelAsync(
            ProductToMapSearchModel searchModel, ProductSEOTemplate productSEOTemplate);

        Task<ProductToMapModel> PrepareProductToMapModelAsync(Product product,
            bool excludeProperties = false);

        Task<ProductToMapListModel> PrepareProductToMapListModelAsync(ProductToMapSearchModel searchModel);

        #endregion

    }
}
