using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Models.Common;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    public class StartupApiController : BaseApiController
    {
        private readonly IApiDeviceService _deviceService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;

        public StartupApiController(IApiDeviceService deviceService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService)
        {
            _deviceService = deviceService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
        }

        [HttpPost]
        [Route("api/appstart")]
        public async Task<IActionResult> AppStart([FromBody] BaseQueryModel<AppStartModel> queryModel)
        {
            var model = queryModel.Data;

            var deviceId = Request.GetAppDeviceId();
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var device = await _deviceService.GetApiDeviceByDeviceIdAsync(deviceId, storeId);

            if (device != null)
            {
                device.CustomerId = customer.Id;
                device.SubscriptionId = model.SubscriptionId;
                device.UpdatedOnUtc = DateTime.UtcNow;
                device.DeviceTypeId = model.DeviceTypeId;
                device.IsRegistered = !await _customerService.IsRegisteredAsync(customer);

                await _deviceService.UpdateApiDeviceAsync(device);
            }
            else
            {
                var newDevice = new ApiDevice
                {
                    CustomerId = customer.Id,
                    DeviceToken = deviceId,
                    SubscriptionId = model.SubscriptionId,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    DeviceTypeId = model.DeviceTypeId,
                    IsRegistered = !await _customerService.IsRegisteredAsync(customer),
                    StoreId = storeId
                };

                await _deviceService.InsertApiDeviceAsync(newDevice);
            }

            return Ok();
        }
    }
}
