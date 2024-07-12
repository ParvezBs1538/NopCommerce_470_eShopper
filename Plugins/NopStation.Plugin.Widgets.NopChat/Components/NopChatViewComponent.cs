using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Vendors;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.NopChat.Models;
using NopStation.Plugin.Widgets.NopChat.Services;

namespace NopStation.Plugin.Widgets.NopChat.Components
{
    public class WidgetsNopChatViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly INopChatMessageService _chatNopMessageService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public WidgetsNopChatViewComponent(IWorkContext workContext,
            INopChatMessageService chatNopMessageService,
            ISettingService settingService,
            IStoreContext storeContext,
            ICustomerService customerService,
            IVendorService vendorService)
        {
            _workContext = workContext;
            _chatNopMessageService = chatNopMessageService;
            _settingService = settingService;
            _storeContext = storeContext;
            _customerService = customerService;
            _vendorService = vendorService;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var nopChatSettings = await _settingService.LoadSettingAsync<NopChatSettings>(storeScope);

            var dayOfWeek = DateTime.Now.ToString("dddd", new CultureInfo("en-US"));
            var chatOpen = false;

            switch (dayOfWeek)
            {
                case "Monday":
                    chatOpen = nopChatSettings.OpenOnMonday;
                    break;

                case "Tuesday":
                    chatOpen = nopChatSettings.OpenOnTuesday;
                    break;

                case "Wednesday":
                    chatOpen = nopChatSettings.OpenOnWednesday;
                    break;

                case "Thursday":
                    chatOpen = nopChatSettings.OpenOnThursday;
                    break;

                case "Friday":
                    chatOpen = nopChatSettings.OpenOnFriday;
                    break;

                case "Saturday":
                    chatOpen = nopChatSettings.OpenOnSaturday;
                    break;

                case "Sunday":
                    chatOpen = nopChatSettings.OpenOnSunday;
                    break;

                default:
                    chatOpen = false;
                    break;
            }

            if (HttpContext.User.Identity.IsAuthenticated && chatOpen == true)
            {
                var customer = (await _workContext.GetCurrentCustomerAsync());
                var customerId = customer.Id;
                var model = new NopChatMessageModel();
                var contactList = await _chatNopMessageService.GetCustomerChatListListAsync(customerId);
                model.ContactList = contactList;
                model.CustomerId = customerId;

                if (widgetZone == PublicWidgetZones.VendorDetailsTop)
                {
                    if (additionalData != null)
                    {
                        var vendorModel = additionalData as VendorModel;
                        model.VendorId = vendorModel.Id;
                    }

                    if (!await _customerService.IsVendorAsync(customer))
                    {
                        return View("~/Plugins/NopStation.Plugin.Widgets.NopChat/Views/PublicInfo.cshtml", model);
                    }
                    else
                    {
                        return Content("");
                    }
                }
                else if (widgetZone == PublicWidgetZones.ProductDetailsInsideOverviewButtonsAfter)
                {
                    var productDetails = additionalData as ProductDetailsModel;
                    var vendor = await _vendorService.GetVendorByIdAsync(productDetails.VendorModel.Id);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        model.VendorId = vendor.Id;
                        return View("~/Plugins/NopStation.Plugin.Widgets.NopChat/Views/_ChatWithVendorButton.cshtml", model);
                    }

                    return Content("");
                }
                else
                {
                    if (!await _customerService.IsVendorAsync(customer))
                    {
                        return View("~/Plugins/NopStation.Plugin.Widgets.NopChat/Views/PublicChatBox.cshtml", model);
                    }
                    else
                    {
                        return Content("");
                    }
                }
            }
            else
            {
                return Content("");
            }
        }
    }
}
