using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;
using NopStation.Plugin.Widgets.FAQ.Domains;
using NopStation.Plugin.Widgets.FAQ.Services;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Factories
{
    public partial class FAQCategoryModelFactory : IFAQCategoryModelFactory
    {
        #region Fields

        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IFAQCategoryService _categoryService;

        #endregion

        #region Ctor

        public FAQCategoryModelFactory(IFAQCategoryService categoryService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService)
        {
            _categoryService = categoryService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public virtual Task<FAQCategorySearchModel> PrepareFAQCategorySearchModelAsync(FAQCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async virtual Task<FAQCategoryListModel> PrepareFAQCategoryListModelAsync(FAQCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get carousels
            var categories = await _categoryService.SearchFAQCategoriesAsync(
                searchModel.SearchName,
                0, null,
                searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new FAQCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async cat =>
                {
                    return await PrepareFAQCategoryModelAsync(null, cat, true);
                });
            });

            return await model;
        }

        public async Task<FAQCategoryModel> PrepareFAQCategoryModelAsync(FAQCategoryModel model, FAQCategory category, bool excludeProperties = false)
        {
            Func<FAQCategoryLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (category != null)
            {
                if (model == null)
                {
                    model = category.ToModel<FAQCategoryModel>();
                }

                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Description = await _localizationService.GetLocalizedAsync(category, entity => entity.Description, languageId, false, false);
                        locale.Name = await _localizationService.GetLocalizedAsync(category, entity => entity.Name, languageId, false, false);
                    };
                }
            }

            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, category, excludeProperties);
            }

            return model;
        }

        #endregion
    }
}
