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
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategorySEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using Nop.Web.Framework.Extensions;
using System.Globalization;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Services;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.AdvancedSEO.Helpers;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public class CategorySEOTemplateModelFactory : ICategorySEOTemplateModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryCategorySEOTemplateMappingModelFactory _categoryCategorySEOTemplateMappingModelFactory;
        private readonly ICategorySEOTemplateService _categorySEOTemplateService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISEOTokenProvider _sEOTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public CategorySEOTemplateModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryCategorySEOTemplateMappingModelFactory categoryCategorySEOTemplateMappingModelFactory,
            ICategorySEOTemplateService categorySEOTemplateService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            ISEOTokenProvider sEOTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryCategorySEOTemplateMappingModelFactory = categoryCategorySEOTemplateMappingModelFactory;
            _categorySEOTemplateService = categorySEOTemplateService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
            _sEOTokenProvider = sEOTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods
        public async Task<CategorySEOTemplateModel> PrepareCategorySEOTemplateModelAsync(CategorySEOTemplateModel model, CategorySEOTemplate categorySEOTemplate,
            bool excludeProperties = false)
        {
            if ( model == null )
                throw new ArgumentNullException(nameof(model));

            Func<SEOTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (categorySEOTemplate != null)
            {
                model = categorySEOTemplate.ToModel<CategorySEOTemplateModel>();
                model.CreatedOn = (await _dateTimeHelper.ConvertToUserTimeAsync(categorySEOTemplate.CreatedOnUtc, DateTimeKind.Utc)).ToString(CultureInfo.InvariantCulture);
                model.UpdatedOn = (await _dateTimeHelper.ConvertToUserTimeAsync(categorySEOTemplate.UpdatedOnUtc, DateTimeKind.Utc)).ToString(CultureInfo.InvariantCulture);

                var lastUpdatedByCustomer = await _customerService.GetCustomerByIdAsync(categorySEOTemplate.LastUpdatedByCustomerId);
                model.LastUpdatedByCustomer = lastUpdatedByCustomer?.Email ?? string.Empty;

                var createdByCustomer = await _customerService.GetCustomerByIdAsync(categorySEOTemplate.CreatedByCustomerId);
                model.CreatedByCustomer = createdByCustomer?.Email ?? string.Empty;

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.SEOTitleTemplate = await _localizationService.GetLocalizedAsync(categorySEOTemplate, entity => entity.SEOTitleTemplate, languageId, false, false);
                    locale.SEODescriptionTemplate = await _localizationService.GetLocalizedAsync(categorySEOTemplate, entity => entity.SEODescriptionTemplate, languageId, false, false);
                    locale.SEOKeywordsTemplate = await _localizationService.GetLocalizedAsync(categorySEOTemplate, entity => entity.SEOKeywordsTemplate, languageId, false, false);
                };

                if (!excludeProperties)
                {
                    model.CategoryCategorySEOTemplateMappingSearchModel = await _categoryCategorySEOTemplateMappingModelFactory.PrepareCategoryCategorySEOTemplateMappingSearchModelAsync(new CategoryCategorySEOTemplateMappingSearchModel(), categorySEOTemplate);
                }
            }

            if (!excludeProperties)
            {
                //prepare model stores
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, categorySEOTemplate, excludeProperties);
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                model.AvailableCategoryMetaTitleTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.CategoryMateTitleTokens));
                model.AvailableCategoryMetaDescriptionTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.CategoryDescriptionTokens));
                model.AvailableCategoryMetaKeywordsTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.CategoryKeywordsTokens));
            }


            return model;
        }

        /// <summary>
        /// Prepare categorySEOTemplate search model
        /// </summary>
        /// <param name="searchModel">CategorySEOTemplate search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categorySEOTemplate search model
        /// </returns>
        public virtual async Task<CategorySEOTemplateSearchModel> PrepareCategorySEOTemplateSearchModelAsync(CategorySEOTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.AvailableStatus = (await SEOTemplateStatusType.Active.ToSelectListAsync()).ToList();
            searchModel.AvailableStatus.Insert(0, new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchStatus.All") });

            searchModel.AvailableTemplateType = (await SEOTemplateType.GlobalTemplate.ToSelectListAsync()).ToList();
            searchModel.AvailableTemplateType.Insert(0, new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchTemplateType.All") });

            searchModel.HideStoresList = searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CategorySEOTemplateListModel> PrepareCategorySEOTemplateListModelAsync(CategorySEOTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            bool? isGlobalTemplate = null;
            if(searchModel.SearchTemplateType > 0)
            {
                if(searchModel.SearchTemplateType == (int)SEOTemplateType.GlobalTemplate)
                    isGlobalTemplate = true;

                else if(searchModel.SearchTemplateType == (int)SEOTemplateType.EntityMappingSpecific)
                    isGlobalTemplate = false;
            }
            bool? isActive = null;
            if(searchModel.SearchStatus > 0)
            {
                if(searchModel.SearchStatus == (int)SEOTemplateStatusType.Active)
                    isActive = true;

                else if(searchModel.SearchStatus == (int)SEOTemplateStatusType.Inactive)
                    isActive = false;
            }

            //get categorySEOTemplates
            var categorySEOTemplates = await _categorySEOTemplateService.GetAllCategorySEOTemplateAsync(
                name : searchModel.SearchTemplateName,
                storeId: searchModel.SearchStoreId,
                isGlobalTemplate: isGlobalTemplate,
                isActive: isActive,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CategorySEOTemplateListModel().PrepareToGridAsync(searchModel, categorySEOTemplates, () =>
            {
                return categorySEOTemplates.SelectAwait(async categorySEOTemplate =>
                {
                    ////fill in model values from the entity
                    //var categorySEOTemplateModel = categorySEOTemplate.ToModel<CategorySEOTemplateModel>();
                    //categorySEOTemplateModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(categorySEOTemplate.CreatedOnUtc, DateTimeKind.Utc);
                    //categorySEOTemplateModel.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(categorySEOTemplate.UpdatedOnUtc, DateTimeKind.Utc);
                    return await PrepareCategorySEOTemplateModelAsync(new CategorySEOTemplateModel(), categorySEOTemplate, true);
                });
            });

            return model;
        }
        #endregion
    }
}
