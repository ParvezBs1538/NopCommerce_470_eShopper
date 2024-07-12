using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Components
{
    public class WebApiCategoryIconAdminViewComponent : NopStationViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryIconModelFactory _categoryIconModelFactory;

        public WebApiCategoryIconAdminViewComponent(ICategoryService categoryService,
            ICategoryIconModelFactory categoryIconModelFactory)
        {
            _categoryService = categoryService;
            _categoryIconModelFactory = categoryIconModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(CategoryModel))
                return Content("");

            var categoryModel = additionalData as CategoryModel;

            var category = await _categoryService.GetCategoryByIdAsync(categoryModel.Id);
            if (category == null || category.Deleted)
                return View(new CategoryIconModel());

            var model = await _categoryIconModelFactory.PrepareCategoryIconModelAsync(null, category);
            return View(model);
        }
    }
}
