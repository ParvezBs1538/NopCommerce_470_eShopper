using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Widgets.DMS.Extensions;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Controllers.API
{
    [Route("api/shipperdevice")]
    public class ShipperDeviceApiController : BaseApiController
    {
        #region Fields


        private readonly IShipperDeviceService _deviceService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public ShipperDeviceApiController(IShipperDeviceService deviceService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService)
        {
            _deviceService = deviceService;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods
        [HttpPost("updatedevicesubscriptionid")]
        public async Task<IActionResult> UpdateDeviceSubscriptionId([FromBody] BaseQueryModel<DeviceSubscriptionModel> queryModel)
        {
            var model = queryModel.Data;

            if (model == null || string.IsNullOrEmpty(model.SubscriptionId) || model.DeviceTypeId < 5)
                return BadRequest("Post Data is not valid");
            //var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var customer = await _workContext.GetCurrentCustomerAsync();

            var isRegistered = await _customerService.IsRegisteredAsync(customer);
            if (!isRegistered)
                return Unauthorized(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

            var deviceId = Request.GetAppDeviceId();
            if (string.IsNullOrEmpty(deviceId))
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound"));

            var device = await _deviceService.GetShipperDeviceByCustomerAsync(customer);
            if (device == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotRegistered"));

            if (device != null)
            {
                if (device.DeviceToken != deviceId)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotValid"));

                device.Online = true;
                device.SubscriptionId = model.SubscriptionId;
                device.UpdatedOnUtc = DateTime.UtcNow;
                device.DeviceTypeId = model.DeviceTypeId;

                await _deviceService.UpdateShipperDeviceAsync(device);
                return Ok();
            }

            return BadRequest();
        }
        [HttpPost("updatedevicelocation")]
        public async Task<IActionResult> UpdateDeviceLocation([FromBody] BaseQueryModel<DeviceModel> queryModel)
        {
            var model = queryModel.Data;

            if (model == null)
                return BadRequest("Post Data is not valid");
            //var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var customer = await _workContext.GetCurrentCustomerAsync();

            var isRegistered = await _customerService.IsRegisteredAsync(customer);
            if (!isRegistered)
                return Unauthorized(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

            var deviceId = Request.GetAppDeviceId();
            if (string.IsNullOrEmpty(deviceId))
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound"));

            var device = await _deviceService.GetShipperDeviceByCustomerAsync(customer);
            if (device == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotRegistered"));

            if (device != null)
            {
                if (device.DeviceToken != deviceId)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotValid"));

                device.Latitude = model.Latitude;
                device.Longitude = model.Longitude;
                device.LocationUpdatedOnUtc = DateTime.UtcNow;

                await _deviceService.UpdateShipperDeviceAsync(device);
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("updateonlinestatus")]
        public async Task<IActionResult> UpdateOnlineStatus([FromBody] BaseQueryModel<DeviceModel> queryModel)
        {
            var model = queryModel.Data;

            if (model == null)
                return BadRequest("Post Data is not valid");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var isRegistered = await _customerService.IsRegisteredAsync(customer);
            if (!isRegistered)
                return Unauthorized(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return Unauthorized(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

            var deviceId = Request.GetAppDeviceId();
            if (string.IsNullOrEmpty(deviceId))
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound"));

            var device = await _deviceService.GetShipperDeviceByCustomerAsync(customer);
            if (device == null)
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotRegistered"));

            if (device != null)
            {
                if (device.DeviceToken != deviceId)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotValid"));

                device.Online = model.Online;

                await _deviceService.UpdateShipperDeviceAsync(device);
                return Ok();
            }

            return BadRequest("device not found");
        }

        #endregion
    }
}
