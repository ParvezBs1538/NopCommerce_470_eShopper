using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public class DeliverySchedulerModelFactory : IDeliverySchedulerModelFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly ISpecialDeliveryOffsetService _specialDeliveryOffsetService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICategoryModelFactory _categoryModelFactory;

        public DeliverySchedulerModelFactory(ICategoryService categoryService,
            ISpecialDeliveryOffsetService specialDeliveryOffsetService,
            ISettingService settingService,
            IStoreContext storeContext,
            ICategoryModelFactory categoryModelFactory)
        {
            _categoryService = categoryService;
            _specialDeliveryOffsetService = specialDeliveryOffsetService;
            _settingService = settingService;
            _storeContext = storeContext;
            _categoryModelFactory = categoryModelFactory;
        }

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<DeliverySchedulerSettings>(storeId);

            var model = settings.ToSettingsModel<ConfigurationModel>();
            model.CategorySearchModel = await _categoryModelFactory.PrepareCategorySearchModelAsync(new CategorySearchModel());

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            model.EnableScheduling_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableScheduling, storeId);
            model.NumberOfDaysToDisplay_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.NumberOfDaysToDisplay, storeId);
            model.DisplayDayOffset_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DisplayDayOffset, storeId);
            model.DateFormat_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DateFormat, storeId);
            model.ShowRemainingCapacity_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowRemainingCapacity, storeId);
            
            return model;
        }

        public async Task<SpecialDeliveryOffsetListModel> PrepareOffsetListModelAsync(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get categories
            var categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            //prepare list model
            var model = await new SpecialDeliveryOffsetListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async category =>
                {
                    var m = new SpecialDeliveryOffsetModel();
                    m.CategoryName = await _categoryService.GetFormattedBreadCrumbAsync(category);
                    m.Id = category.Id;

                    var offset = await _specialDeliveryOffsetService.GetSpecialDeliveryOffsetByIdCategoryAsync(category.Id);
                    if (offset != null)
                    {
                        m.DaysOffset = offset.DaysOffset;
                        m.Overridden = true;
                    }

                    return m;
                });
            });

            return model;
        }
    }
}
