using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.VendorShop.Models;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Components
{
    public class VendorShopHeaderViewComponent : NopStationViewComponent
    {
        private readonly IPictureService _pictureService;
        private readonly IVendorProfileService _vendorProfileService;
        private readonly IStoreContext _storeContext;
        private readonly IVendorSubscriberService _vendorSubscriberService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly VendorShopSettings _vendorShopSettings;

        public VendorShopHeaderViewComponent(
            IPictureService pictureService,
            IVendorProfileService vendorProfileService,
            IStoreContext storeContext,
            IVendorSubscriberService vendorSubscriberService,
            IWorkContext workContext,
            ICustomerService customerService,
            VendorShopSettings vendorShopSettings)
        {
            _pictureService = pictureService;
            _vendorProfileService = vendorProfileService;
            _storeContext = storeContext;
            _vendorSubscriberService = vendorSubscriberService;
            _workContext = workContext;
            _customerService = customerService;
            _vendorShopSettings = vendorShopSettings;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var vendorModel = (VendorModel)additionalData;

            var store = await _storeContext.GetCurrentStoreAsync();

            var vendorProfile = await _vendorProfileService.GetVendorProfileAsync(vendorModel.Id, store.Id) ?? new Domains.VendorProfile();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var isRegisterCustomer = await _customerService.IsRegisteredAsync(currentCustomer);
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var vendorShopHeaderModel = new VendorShopHeaderModel()
            {
                VendorId = vendorModel.Id,
                VendorName = vendorModel.Name,
                ProfilePictureUrl = await _pictureService.GetPictureUrlAsync(vendorProfile.ProfilePictureId),
                BannerPictureUrl = await _pictureService.GetPictureUrlAsync(vendorProfile.BannerPictureId),
                MobileBannerPictureUrl = await _pictureService.GetPictureUrlAsync(vendorProfile.MobileBannerPictureId),
                IsRegisteredCustomer = isRegisterCustomer,
                Description = vendorProfile.Description,
                AllowCustomersToContactVendors = vendorModel.AllowCustomersToContactVendors,
                EnableSubscribeFeature = _vendorShopSettings.EnableVendorShopCampaign,
                IsCurrentCustomerSubscribed = isRegisterCustomer ? await _vendorSubscriberService.IsSubscribedAsync(vendorModel.Id, currentCustomer.Id, storeId) : false,
            };

            return View(vendorShopHeaderModel);
        }

    }
}
