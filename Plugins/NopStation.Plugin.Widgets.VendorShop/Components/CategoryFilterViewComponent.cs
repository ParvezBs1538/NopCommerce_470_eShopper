using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.VendorShop.Services;
using NopStation.Plugin.Widgets.VendorShop.Services.Cache;

namespace NopStation.Plugin.Widgets.VendorShop.Components
{
    public class CategoryFilterViewComponent : NopStationViewComponent
    {
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IVendorCategoryService _vendorCategoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;

        public CategoryFilterViewComponent(IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IVendorCategoryService vendorCategoryService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService)
        {
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _vendorCategoryService = vendorCategoryService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int currentVendorId)
        {
            var categories = await PrepareCategorySimpleModelsAsync(currentVendorId);
            var model = new CategoryNavigationModel
            {
                CurrentCategoryId = 0,
                Categories = categories
            };
            return View(model);
        }

        private async Task<List<CategorySimpleModel>> PrepareCategorySimpleModelsAsync(int vendorId)
        {
            //load and cache them
            var storeId = (await _storeContext.GetCurrentStoreAsync())?.Id ?? 0;
            var cacheKey = VendorCategoryFilterCacheDefault.GetAllVendorCategoryCacheKey(vendorId, storeId);

            var categories = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                return await _vendorCategoryService.GetAllCategoriesByVendorId(vendorId);
            });
            return await PrepareCategorySimpleModelsAsync(categories);
        }
        private async Task<List<CategorySimpleModel>> PrepareCategorySimpleModelsAsync(IList<Category> categories, bool loadSubCategories = true)
        {
            var result = new List<CategorySimpleModel>();

            foreach (var category in categories)
            {
                var categoryModel = new CategorySimpleModel
                {
                    Id = category.Id,
                    Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(category),
                    IncludeInTopMenu = category.IncludeInTopMenu
                };

                if (loadSubCategories)
                {
                    var subCategories = await PrepareCategorySimpleModelsAsync(category.Id);
                    categoryModel.SubCategories.AddRange(subCategories);
                }

                categoryModel.HaveSubCategories = categoryModel.SubCategories.Count > 0 &
                    categoryModel.SubCategories.Any(x => x.IncludeInTopMenu);

                result.Add(categoryModel);
            }

            return result;
        }
    }
}
