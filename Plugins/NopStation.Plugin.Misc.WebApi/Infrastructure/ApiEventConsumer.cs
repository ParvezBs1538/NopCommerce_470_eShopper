using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Infrastructure
{
    public class ApiEventConsumer : IConsumer<CustomerLoggedOutEvent>,
        IConsumer<CustomerLoggedinEvent>,
        IConsumer<OrderPlacedEvent>
    {
        private readonly IApiDeviceService _deviceService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly ICustomerApiService _customerApiService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;

        public ApiEventConsumer(IApiDeviceService apiDeviceService,
            IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService,
            ICustomerApiService customerApiService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IOrderService orderService)
        {
            _deviceService = apiDeviceService;
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _customerApiService = customerApiService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _orderService = orderService;
        }

        public async Task HandleEventAsync(CustomerLoggedOutEvent eventMessage)
        {
            if (_httpContextAccessor.HttpContext.Request.Headers
                .TryGetValue(WebApiCustomerDefaults.DeviceId, out StringValues headerValues))
            {
                var deviceId = headerValues.FirstOrDefault();
                var device = await _deviceService.GetApiDeviceByDeviceIdAsync(deviceId, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (device != null)
                {
                    device.CustomerId = eventMessage.Customer.Id;
                    device.IsRegistered = false;
                    await _deviceService.UpdateApiDeviceAsync(device);
                }
            }
        }

        public async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            if (_httpContextAccessor.HttpContext.Request.Headers
                .TryGetValue(WebApiCustomerDefaults.DeviceId, out StringValues headerValues))
            {
                var deviceId = headerValues.FirstOrDefault();
                var device = await _deviceService.GetApiDeviceByDeviceIdAsync(deviceId, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (device != null)
                {
                    device.CustomerId = eventMessage.Customer.Id;
                    device.IsRegistered = true;
                    await _deviceService.UpdateApiDeviceAsync(device);
                }

                var customerGuid = HelperExtension.GetGuid(deviceId);
                var customer = await _customerService.GetCustomerByGuidAsync(customerGuid);
                if (customer != null)
                {
                    customer.CustomerGuid = Guid.NewGuid();
                    await _customerService.UpdateCustomerAsync(customer);
                }
            }
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;

            if (_httpContextAccessor.HttpContext.Request.Headers
                .TryGetValue(WebApiCustomerDefaults.DeviceId, out StringValues headerValues))
            {
                var deviceId = headerValues.FirstOrDefault();
                var device = await _deviceService.GetApiDeviceByDeviceIdAsync(deviceId, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (device == null)
                    return;

                if (device.DeviceType == Domains.DeviceType.Android)
                {
                    var orderNote = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        OrderId = order.Id,
                        Note = await _localizationService.GetResourceAsync("NopStation.WebApi.Order.PlacedFromAndroid")
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);
                }
                else if (device.DeviceType == Domains.DeviceType.IPhone)
                {
                    var orderNote = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        OrderId = order.Id,
                        Note = await _localizationService.GetResourceAsync("NopStation.WebApi.Order.PlacedFromIPhone")
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);
                }
                else if (device.DeviceType == Domains.DeviceType.Huawei)
                {
                    var orderNote = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        OrderId = order.Id,
                        Note = await _localizationService.GetResourceAsync("NopStation.WebApi.Order.PlacedFromHuawei")
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);
                }
            }
            else
            {
                var orderNote = new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    OrderId = order.Id,
                    Note = await _localizationService.GetResourceAsync("NopStation.WebApi.Order.PlacedFromWeb")
                };
                await _orderService.InsertOrderNoteAsync(orderNote);
            }
        }
    }
}
