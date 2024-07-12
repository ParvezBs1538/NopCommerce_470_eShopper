using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.VendorShop.Models;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Components
{
    public class VendorShopCustomCssViewComponent : NopStationViewComponent
    {
        private readonly IVendorProfileService _vendorProfileService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;

        public VendorShopCustomCssViewComponent(IVendorProfileService vendorProfileService,
            IStoreContext storeContext,
            ILogger logger)
        {
            _vendorProfileService = vendorProfileService;
            _storeContext = storeContext;
            _logger = logger;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var routeData = Url.ActionContext.RouteData;

            try
            {
                var controller = routeData.Values["controller"];
                var action = routeData.Values["action"];
                if (controller == null || action == null)
                    return Content("");

                if (controller.ToString().Equals("CatalogExtension", StringComparison.InvariantCultureIgnoreCase) &&
                    action.ToString().Equals("Vendor", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (int.TryParse(routeData.Values["VendorId"].ToString(), out var vendorId))
                    {
                        var store = await _storeContext.GetCurrentStoreAsync();

                        var vendorProfile = await _vendorProfileService.GetVendorProfileAsync(vendorId, store.Id) ?? new Domains.VendorProfile();
                        if (vendorProfile == null)
                        {
                            return Content("");

                        }
                        return string.IsNullOrEmpty(vendorProfile.CustomCss) ? Content(string.Empty) : View(new CustomCssModel
                        {
                            Css = vendorProfile.CustomCss
                        });
                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, "Vendor Shop Custom CSS", ex.Message);
                return Content("");
            }
        }
    }
}
