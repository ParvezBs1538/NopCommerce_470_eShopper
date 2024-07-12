using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.VendorAndState.Services;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Vendors;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Components
{
    public class VendorShippingViewComponent : NopStationViewComponent
    {
        private readonly IShippingService _shippingService;
        private readonly IVendorShippingService _vendorShippingService;
        private readonly IVendorShippingFactory _vendorShippingFactory;

        public VendorShippingViewComponent(IShippingService shippingService,
            IVendorShippingService vendorShippingService,
            IVendorShippingFactory vendorShippingFactory)
        {
            _shippingService = shippingService;
            _vendorShippingService = vendorShippingService;
            _vendorShippingFactory = vendorShippingFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(VendorModel))
                return Content("");

            var vm = additionalData as VendorModel;
            if(vm.Id == 0)
                return Content("");

            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            if (!shippingMethods.Any())
                return Content("");

            var vendorShipping = await _vendorShippingService.GetVendorShippingByVendorIdAndShippingMethodIdAsync(vm.Id, shippingMethods.First().Id);
            var model = await _vendorShippingFactory.PrepareVendorShippingModelAsync(vendorShipping != null ? null:
                new Models.VendorShippingModel(), vendorShipping);
            model.VendorId = vm.Id;

            return View(model);
        }
    }
}
