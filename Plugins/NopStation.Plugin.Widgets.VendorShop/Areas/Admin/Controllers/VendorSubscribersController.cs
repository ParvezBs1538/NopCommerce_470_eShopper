using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Controllers
{
    public class VendorSubscribersController : NopStationAdminController
    {
        private readonly IVendorSubscriberModelFactory _vendorSubscriberModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IVendorShopNotificationService _vendorShopNotificationService;
        private readonly IStoreContext _storeContext;
        private readonly IVendorSubscriberService _vendorSubscriberService;
        private readonly IWorkContext _workContext;
        private readonly VendorShopSettings _vendorShopSettings;
        private readonly IVendorShopFeatureService _vendorShopFeatureService;

        public VendorSubscribersController(IVendorSubscriberModelFactory vendorSubscriberModelFactory,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IVendorShopNotificationService vendorShopNotificationService,
            IStoreContext storeContext,
            IVendorSubscriberService vendorSubscriberService,
            IWorkContext workContext,
            VendorShopSettings vendorShopSettings,
            IVendorShopFeatureService vendorShopFeatureService)
        {
            _vendorSubscriberModelFactory = vendorSubscriberModelFactory;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _vendorShopNotificationService = vendorShopNotificationService;
            _storeContext = storeContext;
            _vendorSubscriberService = vendorSubscriberService;
            _workContext = workContext;
            _vendorShopSettings = vendorShopSettings;
            _vendorShopFeatureService = vendorShopFeatureService;
        }
        private async Task<bool> CurrentCustomerIsVendorAsync()
        {
            return (await _workContext.GetCurrentVendorAsync()) != null;
        }
        public async Task<IActionResult> List()
        {
            if (!await CurrentCustomerIsVendorAsync() || !_vendorShopSettings.EnableVendorShopCampaign)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null || !await _vendorShopFeatureService.IsEnableVendorShopAsync(currentVendor.Id))
                return RedirectToAction("Index", "Home");

            var searchModel = await _vendorSubscriberModelFactory.PrepareVendorSubscriberSearchModelAsync(new VendorSubscriberSearchModel());
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            searchModel.ActiveStoreScopeConfiguration = storeId;
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> List(VendorSubscriberSearchModel searchModel)
        {
            if (!await CurrentCustomerIsVendorAsync() || !_vendorShopSettings.EnableVendorShopCampaign)
            {
                return await AccessDeniedDataTablesJson();
            }
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null || !await _vendorShopFeatureService.IsEnableVendorShopAsync(currentVendor.Id))
                return await AccessDeniedDataTablesJson();
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            searchModel.ActiveStoreScopeConfiguration = storeId;
            var model = await _vendorSubscriberModelFactory.PrepareVendorSubscriberListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> EmailToSubscribers(VendorCampaignModel model)
        {
            if (!await CurrentCustomerIsVendorAsync() || !_vendorShopSettings.EnableVendorShopCampaign || !await _vendorShopFeatureService.IsEnableVendorShopAsync((await _workContext.GetCurrentVendorAsync())?.Id ?? 0))
            {
                return Json(new
                {
                    success = false,
                    message = "You do not have permission"
                });
            }
            if (model == null)
                return RedirectToAction("List");

            if (!model.SendToAll && (model.SelectedIds == null || model.SelectedIds.Count == 0))
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Alert.NothingSelected") });

            if (string.IsNullOrEmpty(model.ToName) || string.IsNullOrEmpty(model.Subject) || string.IsNullOrEmpty(model.Body))
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Alert.EmptyField") });

            var totalDelayInHours = 0;
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var planedDateTime = model.SendingDate.HasValue ?
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SendingDate.Value) : null;

            if (planedDateTime.HasValue)
            {
                var now = DateTime.UtcNow;
                if (planedDateTime > now)
                {
                    var extraHour = (planedDateTime.Value - now.Date).TotalHours;
                    totalDelayInHours = ((int)extraHour);
                }
                else
                {
                    return Json(new { success = false, message = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Email.SendDate") });
                }
            }

            var messageTemplate = _vendorShopNotificationService.PrepareMessageTemplate(model.ToName, model.Subject, model.Body, totalDelayInHours);

            if (model.SendToAll)
            {
                model.SelectedIds = (await _vendorSubscriberService.GetVendorSubscribersAsync((await _workContext.GetCurrentVendorAsync())?.Id ?? 0))
                    .Select(x => x.Id).ToList();
            }
            if (model.SelectedIds.Any())
                await _vendorShopNotificationService.SendEmailAsync(model.SelectedIds, messageTemplate, storeId);
            else
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Alert.NoSubscriberToSend") });

            return Json(new
            {
                success = true,
                message = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Email.Success"),
            });
        }

    }
}
