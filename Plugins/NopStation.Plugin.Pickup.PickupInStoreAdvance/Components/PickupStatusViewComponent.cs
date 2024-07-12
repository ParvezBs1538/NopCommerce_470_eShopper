using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain.Enum;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Models.Order;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Components
{
    public class PickupStatusViewComponent : NopStationViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public PickupStatusViewComponent(ISettingService settingService,
            IStoreContext storeContext,
            IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {

            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;
            var settings = await _settingService.LoadSettingAsync<PickupInStoreSettings>(storeScope);
            if (!settings.ShowOrderStatusInOrderDetails)
                return Content(string.Empty);

            var orderModel = (OrderDetailsModel)additionalData;
            if (orderModel == null)
            {
                return Content(string.Empty);
            }
            var pickupInStoreDeliveryManage = await _pickupInStoreDeliveryManageService.GetPickupInStoreDeliverManageByOrderIdAsync(orderModel.Id);
            if (pickupInStoreDeliveryManage == null)
            {
                return Content(string.Empty);
            }
            var model = new PickupStatusModel
            {
                StatusId = pickupInStoreDeliveryManage.PickUpStatusTypeId,
                Status = await GetStatusByIdAsync(pickupInStoreDeliveryManage.PickUpStatusTypeId),
                ReadyTime = pickupInStoreDeliveryManage.ReadyForPickupMarkedAtUtc.HasValue ?
                _dateTimeHelper.ConvertToUserTimeAsync(pickupInStoreDeliveryManage.ReadyForPickupMarkedAtUtc.Value, DateTimeKind.Utc).Result.ToString() : string.Empty,
                DeliveryTime = pickupInStoreDeliveryManage.CustomerPickedUpAtUtc.HasValue ?
                _dateTimeHelper.ConvertToUserTimeAsync(pickupInStoreDeliveryManage.CustomerPickedUpAtUtc.Value, DateTimeKind.Utc).Result.ToString() : string.Empty
            };
            return View(model);
        }

        protected async Task<string> GetStatusByIdAsync(int id)
        {
            if (id == (int)PickUpStatusType.OrderInitied)
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.Processing");
            else if (id == (int)PickUpStatusType.ReadyForPick)
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.ReadyForPick");
            else if (id == (int)PickUpStatusType.PickedUpByCustomer)
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.PickedUpByCustomer");
            else if (id == (int)PickUpStatusType.OrderCanceled)
                return await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Status.4");
            else
                return string.Empty;
        }
    }
}
