using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories
{
    public class AjaxFilterModelFactory : IAjaxFilterModelFactory
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IAjaxFilterBaseAdminModelFactory _ajaxFilterBaseAdminModelFactory;
        private readonly IAjaxFilterParentCategoryService _ajaxFilterParentCategoryService;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor
        public AjaxFilterModelFactory(IStoreContext storeContext,
            IAjaxFilterBaseAdminModelFactory ajaxFilterBaseAdminModelFactory,
            IAjaxFilterParentCategoryService ajaxFilterParentCategoryService,
            ICategoryService categoryService,
            ISettingService settingService)
        {
            _storeContext = storeContext;
            _ajaxFilterBaseAdminModelFactory = ajaxFilterBaseAdminModelFactory;
            _ajaxFilterParentCategoryService = ajaxFilterParentCategoryService;
            _categoryService = categoryService;
            _settingService = settingService;
        }
        #endregion

        #region Methods
        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ajaxSettings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeId);
            var model = ajaxSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeId;

            await _ajaxFilterBaseAdminModelFactory.PrepareSpecificationAttributeAsync(model.AvaliableSpecificationAttributes, false);

            if (storeId <= 0)
                return model;

            model.EnableFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableFilter, storeId);
            model.EnableProductCount_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableProductCount, storeId);
            model.HideManufacturerProductCount_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.HideManufacturerProductCount, storeId);
            model.MaxDisplayForCategories_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.MaxDisplayForCategories, storeId);
            model.MaxDisplayForManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.MaxDisplayForManufacturers, storeId);
            model.EnablePriceRangeFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnablePriceRangeFilter, storeId);
            model.EnableCategoryFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableCategoryFilter, storeId);
            model.EnableSpecificationAttributeFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableSpecificationAttributeFilter, storeId);

            model.EnableManufacturerFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableManufacturerFilter, storeId);
            model.CloseManufactureFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseManufactureFilterByDefualt, storeId);

            model.CloseCategoryFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseCategoryFilterByDefualt, storeId);

            model.ClosePriceRangeFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.ClosePriceRangeFilterByDefualt, storeId);

            model.EnableProductRatingsFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableProductRatingsFilter, storeId);
            model.CloseProductRatingsFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseProductRatingsFilterByDefualt, storeId);

            model.EnableProductTagsFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableProductTagsFilter, storeId);
            model.CloseProductTagsFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseProductTagsFilterByDefualt, storeId);


            model.EnableMiscFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableMiscFilter, storeId);
            model.CloseMiscFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseMiscFilterByDefualt, storeId);

            model.EnableProductAttributeFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableProductAttributeFilter, storeId);
            model.CloseProductAttributeFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseProductAttributeFilterByDefualt, storeId);


            model.EnableMiscFilter_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.EnableMiscFilter, storeId);
            model.CloseMiscFilterByDefualt_OverrideForStore = await _settingService.SettingExistsAsync(ajaxSettings, x => x.CloseMiscFilterByDefualt, storeId);

            return model;
        }

        public async Task<AjaxFilterParentCategoryModel> PrepareParentCategoryModelAsync(AjaxFilterParentCategory ajaxFilterParentCategory)
        {
            if (ajaxFilterParentCategory.Id > 0)
            {
                var model = new AjaxFilterParentCategoryModel();
                var category = await _ajaxFilterParentCategoryService.GetParentCategoryByCategoryIdAsync(ajaxFilterParentCategory.CategoryId);
                if (category != null)
                {
                    model.Id = category.Id;
                    model.EnablePriceRangeFiltering = category.EnablePriceRangeFiltering;
                    model.EnableManufactureFiltering = category.EnableManufactureFiltering;
                    model.EnableSpecificationAttributeFiltering = category.EnableSpecificationAttributeFiltering;
                    model.EnableVendorFiltering = category.EnableVendorFiltering;
                    model.EnableSearchForSpecifications = category.EnableSearchForSpecifications;
                    model.EnableSearchForManufacturers = category.EnableSearchForManufacturers;
                    return model;
                }
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var ajaxFilterSettings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeScope);

                model.Id = ajaxFilterParentCategory.CategoryId;
                model.EnablePriceRangeFiltering = ajaxFilterSettings.EnablePriceRangeFilter;
                model.EnableManufactureFiltering = ajaxFilterSettings.EnableManufacturerFilter;
                model.EnableSpecificationAttributeFiltering = ajaxFilterSettings.EnableSpecificationAttributeFilter;

                return model;
            }

            return new AjaxFilterParentCategoryModel();
        }

        public Task<AjaxFilterParentCategorySearchModel> PrepareParentCategorySearchModelAsync(AjaxFilterParentCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<CategoryListModel> PrepareParentCategoryListModelAsync(AjaxFilterParentCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentException(nameof(searchModel));

            var categories = await _ajaxFilterParentCategoryService.GetParentCategoriesAsync(searchModel, searchModel.Page - 1, searchModel.PageSize);

            var model = new CategoryListModel().PrepareToGrid(searchModel, categories, () =>
            {
                return categories.Select(category => new CategoryModel()
                {
                    Id = category.Id,
                    Name = category.Name
                });
            });

            return model;
        }

        public async Task<AjaxFilterParentCategoryListModel> PrepareSelectedParentCategoryListModelAsync(AjaxFilterParentCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentException(nameof(searchModel));

            var categories = await _ajaxFilterParentCategoryService.GetSelectedParentCategoriesAsync(searchModel, searchModel.Page - 1, searchModel.PageSize);

            var model = await new AjaxFilterParentCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
             {
                 return categories.SelectAwait(async category => new AjaxFilterParentCategoryModel()
                 {
                     Id = category.Id,
                     CategoryId = category.CategoryId,
                     CategoryName = (await _categoryService.GetCategoryByIdAsync(category.CategoryId)).Name,
                     EnablePriceRangeFiltering = category.EnablePriceRangeFiltering,
                     EnableManufactureFiltering = category.EnableManufactureFiltering,
                     EnableSpecificationAttributeFiltering = category.EnableSpecificationAttributeFiltering,
                     EnableVendorFiltering = category.EnableVendorFiltering
                 });
             });

            return model;
        }

        public async Task<AjaxFilterParentCategoryListSearchModel> PrepareParentCategoryListSearchModelAsync(AjaxFilterParentCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentException(nameof(searchModel));

            var categories = await _ajaxFilterParentCategoryService.GetParentCategoriesAsync(searchModel, searchModel.Page - 1, searchModel.PageSize);
            var model = new AjaxFilterParentCategoryListSearchModel();
            foreach (var category in categories)
            {
                model.AvaliableCategoryIds.Add(category.Id);
            }
            model.SetGridPageSize(searchModel.PageSize);
            return model;

        }
        #endregion
    }
}
