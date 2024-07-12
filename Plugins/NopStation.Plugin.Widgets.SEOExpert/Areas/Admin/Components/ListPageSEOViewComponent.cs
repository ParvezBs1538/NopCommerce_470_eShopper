using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SEOExpert.Domains;

namespace NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Components
{
    public class ListPageSEOViewComponent : NopStationViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        public ListPageSEOViewComponent(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return Content("");

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<SEOExpertSettings>(storeScope);

            var model = new SEOModel
            {
                EnableListGeneration = settings.EnableListGeneration,
            };

            if (widgetZone == AdminWidgetZones.ProductListButtons)
                model.EntityTypeId = (int)SEOEntityType.Product;
            else if (widgetZone == AdminWidgetZones.CategoryListButtons)
                model.EntityTypeId = (int)SEOEntityType.Category;
            else
                model.EntityTypeId = (int)SEOEntityType.Manufacturer;

            return View(model);
        }
    }
}