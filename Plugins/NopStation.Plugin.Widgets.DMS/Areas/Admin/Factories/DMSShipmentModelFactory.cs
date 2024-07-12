using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public class DMSShipmentModelFactory : IDMSShipmentModelFactory
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IMeasureService _measureService;
        private readonly IOrderService _orderService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IShipmentService _shipmentService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly MeasureSettings _measureSettings;

        #endregion

        #region Ctor

        public DMSShipmentModelFactory(IAddressService addressService,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IMeasureService measureService,
            IOrderService orderService,
            IQrCodeService qrCodeService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            MeasureSettings measureSettings
            )
        {
            _addressService = addressService;
            _countryService = countryService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _measureService = measureService;
            _orderService = orderService;
            _qrCodeService = qrCodeService;
            _shipmentService = shipmentService;
            _stateProvinceService = stateProvinceService;
            _measureSettings = measureSettings;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public virtual async Task<byte[]> GeneratePackagingSlipsToPdfAsync(int shipmentId = 0)
        {
            if (shipmentId == 0)
                return null;

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
                return null;

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            if (order == null)
                return null;

            if (order.PickupInStore || order.ShippingAddressId == null)
                throw new ArgumentException("Order is no shippable");

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            if (customer == null)
                return null;

            var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
            var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);

            if (shippingAddress == null)
                return null;

            var shippingCountry = await _countryService.GetCountryByAddressAsync(shippingAddress);
            var stateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress))?.Name;
            var shipmentTotalWeight = $"{shipment.TotalWeight:F2} [{(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name}]";
            var qrcode = await _qrCodeService.GenetareShippingQrCodeInByteAsync(shipment, order, shippingAddress, customer, customerFullName, shippingCountry.Name, stateProvinceName, shipmentTotalWeight);

            return qrcode;
        }

        #endregion
    }
}
