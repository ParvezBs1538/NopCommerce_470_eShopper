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
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using Nop.Web.Framework.Extensions;
using System.Globalization;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Services;
using Nop.Services.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryToMap;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public class CategoryCategorySEOTemplateMappingModelFactory : ICategoryCategorySEOTemplateMappingModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryCategorySEOTemplateMappingService _categoryCategorySEOTemplateMappingService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public CategoryCategorySEOTemplateMappingModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICategoryCategorySEOTemplateMappingService categoryCategorySEOTemplateMappingService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _categoryCategorySEOTemplateMappingService = categoryCategorySEOTemplateMappingService;
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

        #region CategoryCategorySEOTemplateMapping

        public async Task<CategoryCategorySEOTemplateMappingModel> PrepareCategoryCategorySEOTemplateMappingModelAsync(CategoryCategorySEOTemplateMappingModel model, CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping,
            bool excludeProperties = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (categoryCategorySEOTemplateMapping != null)
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryCategorySEOTemplateMapping.CategoryId);
                if(category == null)
                    throw new ArgumentNullException(nameof(category));

                model = categoryCategorySEOTemplateMapping.ToModel<CategoryCategorySEOTemplateMappingModel>();
                model.CategoryName = await _categoryService.GetFormattedBreadCrumbAsync(category);
            }
            return model;
        }

        /// <summary>
        /// Prepare categoryCategorySEOTemplateMapping search model
        /// </summary>
        /// <param name="searchModel">CategoryCategorySEOTemplateMapping search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categoryCategorySEOTemplateMapping search model
        /// </returns>
        public virtual async Task<CategoryCategorySEOTemplateMappingSearchModel> PrepareCategoryCategorySEOTemplateMappingSearchModelAsync(
            CategoryCategorySEOTemplateMappingSearchModel searchModel, CategorySEOTemplate categorySEOTemplate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (categorySEOTemplate == null)
                throw new ArgumentNullException(nameof(categorySEOTemplate));


            searchModel.CategorySEOTemplateId = categorySEOTemplate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;
            return searchModel;
        }

        public virtual async Task<CategoryCategorySEOTemplateMappingListModel> PrepareCategoryCategorySEOTemplateMappingListModelAsync(CategoryCategorySEOTemplateMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get categoryCategorySEOTemplateMappings
            var categoryCategorySEOTemplateMappings = await _categoryCategorySEOTemplateMappingService.GetAllCategoryCategorySEOTemplateMappingAsync(
                categorySEOTemplateId : searchModel.CategorySEOTemplateId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CategoryCategorySEOTemplateMappingListModel().PrepareToGridAsync(searchModel, categoryCategorySEOTemplateMappings, () =>
            {
                return categoryCategorySEOTemplateMappings.SelectAwait(async categoryCategorySEOTemplateMapping =>
                {
                    return await PrepareCategoryCategorySEOTemplateMappingModelAsync(new CategoryCategorySEOTemplateMappingModel(), categoryCategorySEOTemplateMapping, true);
                });
            });

            return model;
        }

        #endregion

        #region CategoryToMap



        /// <summary>
        /// Prepare categoryCategorySEOTemplateMapping search model
        /// </summary>
        /// <param name="searchModel">CategoryCategorySEOTemplateMapping search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categoryCategorySEOTemplateMapping search model
        /// </returns>
        public virtual async Task<CategoryToMapSearchModel> PrepareCategoryToMapSearchModelAsync(
            CategoryToMapSearchModel searchModel, CategorySEOTemplate categorySEOTemplate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (categorySEOTemplate == null)
                throw new ArgumentNullException(nameof(categorySEOTemplate));

            searchModel.CategorySEOTemplateId = categorySEOTemplate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;
            return searchModel;
        }

        public async Task<CategoryToMapModel> PrepareCategoryToMapModelAsync(Category category,
            bool excludeProperties = false)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            return new CategoryToMapModel(category)
            {
                CategoryBreadCrumb = await _categoryService.GetFormattedBreadCrumbAsync(category)
            };
        }

        public virtual async Task<CategoryToMapListModel> PrepareCategoryToMapListModelAsync(CategoryToMapSearchModel searchModel)
        {
            if(searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get Not Mapped categoties
            var categories = await _categoryCategorySEOTemplateMappingService.GetAllNotMappedCategoriesByCategorySEOTemplateId(
                categoryName: searchModel.CategoryName,
                categorySEOTemplateId : searchModel.CategorySEOTemplateId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CategoryToMapListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async category =>
                {
                    return await PrepareCategoryToMapModelAsync(category, true);
                });
            });

            return model;
        }

        #endregion

        #endregion
    }
}
