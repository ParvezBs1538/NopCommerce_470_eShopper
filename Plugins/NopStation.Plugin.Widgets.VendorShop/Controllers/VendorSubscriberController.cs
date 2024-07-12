using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Controllers
{
    public class VendorSubscriberController : NopStationPublicController
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorSubscriberService _vendorSubscriberService;
        private readonly IStoreContext _storeContext;
        private readonly VendorShopSettings _vendorShopSettings;

        public VendorSubscriberController(
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IVendorSubscriberService vendorSubscriberService,
            IStoreContext storeContext,
            VendorShopSettings vendorShopSettings)
        {
            _customerService = customerService;
            _workContext = workContext;
            _localizationService = localizationService;
            _vendorSubscriberService = vendorSubscriberService;
            _storeContext = storeContext;
            _vendorShopSettings = vendorShopSettings;
        }
        public async Task<IActionResult> Subscribe(int vendorId)
        {
            if (!_vendorShopSettings.EnableVendorShopCampaign)
            {
                return Json(new
                {
                    message = "This feature is not enabled.",
                    success = false,
                    buttonText = "",
                });
            }
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var isGuest = await _customerService.IsGuestAsync(currentCustomer);
            if (isGuest)
            {
                return Challenge();
            }
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var alreadySubscribe = await _vendorSubscriberService.IsSubscribedAsync(vendorId, currentCustomer.Id, storeId);
            if (alreadySubscribe)
            {
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("NopStation.VendorShop.Subscribe.AlreadySubscriber"),
                    success = false,
                    buttonText = await _localizationService.GetResourceAsync("NopStation.VendorShop.Unsubscribe.Text"),
                });
            }
            else
            {
                await _vendorSubscriberService.InsertVendorSubscriberAsync(new VendorSubscriber
                {
                    VendorId = vendorId,
                    CustomerId = currentCustomer.Id,
                    SubscribedOn = DateTime.UtcNow,
                    StoreId = storeId,
                });
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("NopStation.VendorShop.Subscribe.SubscribeSuccess"),
                    success = true,
                    buttonText = await _localizationService.GetResourceAsync("NopStation.VendorShop.Unsubscribe.Text"),
                });
            }
        }
        public async Task<IActionResult> UnSubscribe(int vendorId)
        {
            if (!_vendorShopSettings.EnableVendorShopCampaign)
            {
                return Json(new
                {
                    message = "This feature is not enabled.",
                    success = false,
                    buttonText = "",
                });
            }
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var isGuest = await _customerService.IsGuestAsync(currentCustomer);
            if (isGuest)
            {
                return Challenge();
            }
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var alreadySubscribe = await _vendorSubscriberService.IsSubscribedAsync(vendorId, currentCustomer.Id, storeId);
            if (!alreadySubscribe)
            {
                return Json(new
                {
                    message = await _localizationService.GetResourceAsync("NopStation.VendorShop.Subscribe.NotSubscriber"),
                    success = false,
                    buttonText = await _localizationService.GetResourceAsync("NopStation.VendorShop.Subscribe.Text"),
                });
            }
            else
            {
                var subscriber = await _vendorSubscriberService.GetVendorSubscriberAsync(vendorId, currentCustomer.Id, storeId);
                if (subscriber != null)
                {
                    await _vendorSubscriberService.DeleteVendorSubscriberAsync(subscriber);
                    return Json(new
                    {
                        message = await _localizationService.GetResourceAsync("NopStation.VendorShop.Subscribe.Unsubscribe"),
                        success = true,
                        buttonText = await _localizationService.GetResourceAsync("NopStation.VendorShop.Subscribe.Text"),
                    });
                }
            }
            return Json(new());
        }

    }
}
