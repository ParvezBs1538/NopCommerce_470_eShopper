using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public class ProductProductSEOTemplateMappingModelFactory : IProductProductSEOTemplateMappingModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProductService _productService;
        private readonly IProductProductSEOTemplateMappingService _productProductSEOTemplateMappingService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public ProductProductSEOTemplateMappingModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            IProductService productService,
            IProductProductSEOTemplateMappingService productProductSEOTemplateMappingService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _productService = productService;
            _productProductSEOTemplateMappingService = productProductSEOTemplateMappingService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods 

        #region Product product seo template mapping

        public async Task<ProductProductSEOTemplateMappingModel> PrepareProductProductSEOTemplateMappingModelAsync(ProductProductSEOTemplateMappingModel model, ProductProductSEOTemplateMapping productProductSEOTemplateMapping,
            bool excludeProperties = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (productProductSEOTemplateMapping != null)
            {
                var product = await _productService.GetProductByIdAsync(productProductSEOTemplateMapping.ProductId);
                if(product == null)
                    throw new ArgumentNullException(nameof(product));

                model = productProductSEOTemplateMapping.ToModel<ProductProductSEOTemplateMappingModel>();
                model.ProductName = product.Name;
            }
            return model;
        }

        public virtual async Task<ProductProductSEOTemplateMappingSearchModel> PrepareProductProductSEOTemplateMappingSearchModelAsync(
            ProductProductSEOTemplateMappingSearchModel searchModel, ProductSEOTemplate productSEOTemplate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productSEOTemplate == null)
                throw new ArgumentNullException(nameof(productSEOTemplate));


            searchModel.ProductSEOTemplateId = productSEOTemplate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;
            return searchModel;
        }

        public virtual async Task<ProductProductSEOTemplateMappingListModel> PrepareProductProductSEOTemplateMappingListModelAsync(ProductProductSEOTemplateMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get productProductSEOTemplateMappings
            var productProductSEOTemplateMappings = await _productProductSEOTemplateMappingService.GetAllProductProductSEOTemplateMappingAsync(
                productSEOTemplateId : searchModel.ProductSEOTemplateId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ProductProductSEOTemplateMappingListModel().PrepareToGridAsync(searchModel, productProductSEOTemplateMappings, () =>
            {
                return productProductSEOTemplateMappings.SelectAwait(async productProductSEOTemplateMapping =>
                {
                    return await PrepareProductProductSEOTemplateMappingModelAsync(new ProductProductSEOTemplateMappingModel(), productProductSEOTemplateMapping, true);
                });
            });

            return model;
        }

        #endregion

        #region Product to map

        public virtual async Task<ProductToMapSearchModel> PrepareProductToMapSearchModelAsync(
            ProductToMapSearchModel searchModel, ProductSEOTemplate productSEOTemplate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productSEOTemplate == null)
                throw new ArgumentNullException(nameof(productSEOTemplate));

            searchModel.ProductSEOTemplateId = productSEOTemplate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;
            return searchModel;
        }

        public Task<ProductToMapModel> PrepareProductToMapModelAsync(Product product,
            bool excludeProperties = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            return Task.FromResult(new ProductToMapModel(product)
            {
                //ProductBreadCrumb = await _productService.GetFormattedBreadCrumbAsync(product)
            });
        }

        public virtual async Task<ProductToMapListModel> PrepareProductToMapListModelAsync(ProductToMapSearchModel searchModel)
        {
            if(searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get Not Mapped categoties
            var categories = await _productProductSEOTemplateMappingService.GetAllNotMappedCategoriesByProductSEOTemplateId(
                productName: searchModel.ProductName,
                productSEOTemplateId : searchModel.ProductSEOTemplateId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ProductToMapListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async product =>
                {
                    return await PrepareProductToMapModelAsync(product, true);
                });
            });

            return model;
        }

        #endregion

        #endregion
    }
}
