using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Components;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Components
{
    public class VendorShopFeatureViewComponent : NopViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IVendorShopFeatureService _vendorShopFeatureService;

        public VendorShopFeatureViewComponent(IPermissionService permissionService,
            IStoreContext storeContext,
            IVendorShopFeatureService vendorShopFeatureService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _vendorShopFeatureService = vendorShopFeatureService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageConfiguration) || additionalData == null)
                return Content("");

            var vendorModel = additionalData as VendorModel;
            if (vendorModel == null || vendorModel.Id == 0)
                return Content(string.Empty);

            var vendorShopFeature = await _vendorShopFeatureService.GetVendorFeatureMappingByVendorIdAsync(vendorModel.Id);

            var model = new VendorFeatureMappingModel
            {
                VendorShopEnable = vendorShopFeature?.Enable ?? false,
                VendorId = vendorModel.Id
            };

            return View(model);
        }
    }
}
