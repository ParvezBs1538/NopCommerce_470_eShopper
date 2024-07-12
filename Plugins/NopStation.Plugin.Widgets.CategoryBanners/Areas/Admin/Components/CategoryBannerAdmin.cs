using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Models;
using NopStation.Plugin.Misc.Core.Components;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Components
{
    public class CategoryBannerAdminViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(CategoryModel))
                return Content("");

            var categoryModel = additionalData as CategoryModel;
            var searchModel = new CategoryBannerSearchModel()
            {
                CategoryId = categoryModel.Id
            };
            searchModel.SetGridPageSize();

            return View(searchModel);
        }
    }
}
