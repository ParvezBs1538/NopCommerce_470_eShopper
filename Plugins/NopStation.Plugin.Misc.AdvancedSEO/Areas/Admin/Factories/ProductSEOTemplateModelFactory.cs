using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Factories;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using Nop.Web.Framework.Extensions;
using System.Globalization;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Services;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.AdvancedSEO.Helpers;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public class ProductSEOTemplateModelFactory : IProductSEOTemplateModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProductSEOTemplateService _productSEOTemplateService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IProductProductSEOTemplateMappingModelFactory _productProductSEOTemplateMappingModelFactory;
        private readonly ISEOTokenProvider _sEOTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public ProductSEOTemplateModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            IProductSEOTemplateService productSEOTemplateService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            IProductProductSEOTemplateMappingModelFactory productProductSEOTemplateMappingModelFactory,
            ISEOTokenProvider sEOTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _productSEOTemplateService = productSEOTemplateService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
            _productProductSEOTemplateMappingModelFactory = productProductSEOTemplateMappingModelFactory;
            _sEOTokenProvider = sEOTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task<ProductSEOTemplateModel> PrepareProductSEOTemplateModelAsync(ProductSEOTemplateModel model, ProductSEOTemplate productSEOTemplate,
            bool excludeProperties = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Func<SEOTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (productSEOTemplate != null)
            {
                model = productSEOTemplate.ToModel<ProductSEOTemplateModel>();
                model.CreatedOn = (await _dateTimeHelper.ConvertToUserTimeAsync(productSEOTemplate.CreatedOnUtc, DateTimeKind.Utc)).ToString(CultureInfo.InvariantCulture);
                model.UpdatedOn = (await _dateTimeHelper.ConvertToUserTimeAsync(productSEOTemplate.UpdatedOnUtc, DateTimeKind.Utc)).ToString(CultureInfo.InvariantCulture);

                var lastUpdatedByCustomer = await _customerService.GetCustomerByIdAsync(productSEOTemplate.LastUpdatedByCustomerId);
                model.LastUpdatedByCustomer = lastUpdatedByCustomer?.Email ?? string.Empty;

                var createdByCustomer = await _customerService.GetCustomerByIdAsync(productSEOTemplate.CreatedByCustomerId);
                model.CreatedByCustomer = createdByCustomer?.Email ?? string.Empty;

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.SEOTitleTemplate = await _localizationService.GetLocalizedAsync(productSEOTemplate, entity => entity.SEOTitleTemplate, languageId, false, false);
                    locale.SEODescriptionTemplate = await _localizationService.GetLocalizedAsync(productSEOTemplate, entity => entity.SEODescriptionTemplate, languageId, false, false);
                    locale.SEOKeywordsTemplate = await _localizationService.GetLocalizedAsync(productSEOTemplate, entity => entity.SEOKeywordsTemplate, languageId, false, false);
                };

                if (!excludeProperties)
                {
                    model.ProductProductSEOTemplateMappingSearchModel = await _productProductSEOTemplateMappingModelFactory.PrepareProductProductSEOTemplateMappingSearchModelAsync(new ProductProductSEOTemplateMappingSearchModel(), productSEOTemplate);

                }
            }

            if (!excludeProperties)
            {
                //prepare model stores
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, productSEOTemplate, excludeProperties);
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                model.AvailableProductMetaTitleTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.ProductMateTitleTokens));
                model.AvailableProductMetaDescriptionTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.ProductDescriptionTokens));
                model.AvailableProductMetaKeywordsTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.ProductKeywordsTokens));
            }

            return model;
        }

        /// <summary>
        /// Prepare productSEOTemplate search model
        /// </summary>
        /// <param name="searchModel">ProductSEOTemplate search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the productSEOTemplate search model
        /// </returns>
        public virtual async Task<ProductSEOTemplateSearchModel> PrepareProductSEOTemplateSearchModelAsync(ProductSEOTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);


            searchModel.AvailableStatus = (await SEOTemplateStatusType.Active.ToSelectListAsync()).ToList();
            searchModel.AvailableStatus.Insert(0, new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchStatus.All") });

            searchModel.AvailableTemplateType = (await SEOTemplateType.GlobalTemplate.ToSelectListAsync()).ToList();
            searchModel.AvailableTemplateType.Insert(0, new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchTemplateType.All") });

            searchModel.HideStoresList = searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<ProductSEOTemplateListModel> PrepareProductSEOTemplateListModelAsync(ProductSEOTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            bool? isGlobalTemplate = null;
            if (searchModel.SearchTemplateType > 0)
            {
                if (searchModel.SearchTemplateType == (int)SEOTemplateType.GlobalTemplate)
                    isGlobalTemplate = true;

                else if (searchModel.SearchTemplateType == (int)SEOTemplateType.EntityMappingSpecific)
                    isGlobalTemplate = false;
            }
            bool? isActive = null;
            if (searchModel.SearchStatus > 0)
            {
                if (searchModel.SearchStatus == (int)SEOTemplateStatusType.Active)
                    isActive = true;

                else if (searchModel.SearchStatus == (int)SEOTemplateStatusType.Inactive)
                    isActive = false;
            }

            //get productSEOTemplates
            var productSEOTemplates = await _productSEOTemplateService.GetAllProductSEOTemplateAsync(
                name: searchModel.SearchTemplateName,
                storeId: searchModel.SearchStoreId,
                isGlobalTemplate: isGlobalTemplate,
                isActive: isActive,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ProductSEOTemplateListModel().PrepareToGridAsync(searchModel, productSEOTemplates, () =>
            {
                return productSEOTemplates.SelectAwait(async productSEOTemplate =>
                {
                    return await PrepareProductSEOTemplateModelAsync(new ProductSEOTemplateModel(), productSEOTemplate, true);
                });
            });

            return model;
        }
        #endregion
    }
}
