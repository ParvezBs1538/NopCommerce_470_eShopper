using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.VendorShop.Factories;
using NopStation.Plugin.Widgets.VendorShop.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Controllers
{
    public class CatalogExtensionController : NopStationPublicController
    {
        private readonly IVendorService _vendorService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly IVendorCatalogModelFactory _vendorCatalogModelFactory;

        public CatalogExtensionController(IVendorService vendorService,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IWebHelper webHelper,
            IPermissionService permissionService,
            IVendorCatalogModelFactory vendorCatalogModelFactory)
        {
            _vendorService = vendorService;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _vendorCatalogModelFactory = vendorCatalogModelFactory;
        }

        public virtual async Task<IActionResult> Vendor(int vendorId, CatalogProductsExtensionCommand command)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return InvokeHttp404();

            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                store.Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = AreaNames.ADMIN }));

            //model
            var model = await _vendorCatalogModelFactory.PrepareVendorModelAsync(vendor, command);

            return View(model);
        }


        public virtual async Task<IActionResult> GetCustomVendorProducts(int vendorId, CatalogProductsExtensionCommand command)

        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return NotFound();

            var model = await _vendorCatalogModelFactory.PrepareVendorProductsModelAsync(vendor, command);

            return PartialView("_ProductsInGridOrLines", model);
        }


        private Task<bool> CheckVendorAvailabilityAsync(Vendor vendor)
        {
            var isAvailable = true;

            if (vendor == null || vendor.Deleted || !vendor.Active)
                isAvailable = false;

            return Task.FromResult(isAvailable);
        }
    }
}
