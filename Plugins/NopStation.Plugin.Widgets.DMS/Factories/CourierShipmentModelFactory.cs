using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Shipping;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Factories;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Factories
{
    public class CourierShipmentModelFactory : ICourierShipmentModelFactory
    {
        #region Fields

        private readonly IShipmentService _shipmentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ICourierShipmentService _courierShipmentService;
        private readonly IOrderService _orderService;
        private readonly DMSSettings _dMSSettings;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly IPictureService _pictureService;
        private readonly IShipmentPickupPointService _shipmentPickupPointService;
        private readonly ICustomerService _customerService;
        private readonly MediaSettings _mediaSettings;
        private readonly IDeliverFailedRecordService _deliverFailedRecordService;
        private readonly ShippingSettings _shippingSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public CourierShipmentModelFactory(IShipmentService shipmentService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ICourierShipmentService courierShipmentService,
            IOrderService orderService,
            ShippingSettings shippingSettings,
            CatalogSettings catalogSettings,
            DMSSettings dMSSettings,
            IAddressService addressService,
            ICountryService countryService,
            IAddressModelFactory addressModelFactory,
            IStateProvinceService stateProvinceService,
            IOrderModelFactory orderModelFactory,
            AddressSettings addressSettings,
            IPictureService pictureService,
            IShipmentPickupPointService shipmentPickupPointService,
            ICustomerService customerService,
            MediaSettings mediaSettings,
            IDeliverFailedRecordService deliverFailedRecordService)
        {
            _shipmentService = shipmentService;
            _addressModelFactory = addressModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _courierShipmentService = courierShipmentService;
            _orderService = orderService;
            _dMSSettings = dMSSettings;
            _orderModelFactory = orderModelFactory;
            _addressSettings = addressSettings;
            _pictureService = pictureService;
            _shipmentPickupPointService = shipmentPickupPointService;
            _customerService = customerService;
            _mediaSettings = mediaSettings;
            _deliverFailedRecordService = deliverFailedRecordService;
            _shippingSettings = shippingSettings;
            _catalogSettings = catalogSettings;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities
        protected string FormatAddressString(string address, string addressPart = null)
        {
            if (!string.IsNullOrEmpty(addressPart))
            {
                if (!string.IsNullOrEmpty(address))
                    address += ", ";

                address += addressPart;
            }
            return address;
        }

        protected async Task<string> PrepareAddressStringAsync(Address address)
        {
            if (address == null)
                return null;

            var addressStr = "";
            addressStr = FormatAddressString(addressStr, address.Address1);
            addressStr = FormatAddressString(addressStr, address.Address2);
            addressStr = FormatAddressString(addressStr, address.City);
            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
            if (stateProvince != null)
                addressStr = FormatAddressString(addressStr, stateProvince.Name);
            addressStr = FormatAddressString(addressStr, address.ZipPostalCode);

            var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
            if (country != null)
                addressStr = FormatAddressString(addressStr, country.Name);

            return addressStr;
        }

        protected async Task PrepareShipmentPickupPointInfoAsync(CourierShipmentOverviewModel model, int shipmentPickupPointId)
        {
            if (shipmentPickupPointId < 1)
                throw new ArgumentNullException(nameof(shipmentPickupPointId));

            var shipmentPickupPoint = await _shipmentPickupPointService.GetShipmentPickupPointByIdAsync(shipmentPickupPointId);
            if (shipmentPickupPoint == null)
                return;

            var pickupPointInfo = shipmentPickupPoint.Name;

            var address = await _addressService.GetAddressByIdAsync(shipmentPickupPoint.AddressId);
            if (address == null)
                return;

            var addressStr = await PrepareAddressStringAsync(address);

            if (!string.IsNullOrEmpty(addressStr))
                pickupPointInfo += $" ( {addressStr} )";

            model.PickupPointInfo = pickupPointInfo;
            model.PickupTime = shipmentPickupPoint?.OpeningHours ?? "";

        }


        protected Task PreparePageSizeOptionsAsync(CourierShipmentsModel model, CourierShipmentsModel command)
        {
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            model.AllowShippersToSelectPageSize = false;
            if (_dMSSettings.AllowShippersToSelectPageSize && !string.IsNullOrWhiteSpace(_dMSSettings.PageSizeOptions))
            {
                var pageSizes = _dMSSettings.PageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out var temp))
                        {
                            if (temp > 0)
                                command.PageSize = temp;
                        }
                    }

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out var temp))
                            continue;

                        if (temp <= 0)
                            continue;

                        model.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = pageSize,
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (model.PageSizeOptions.Any())
                    {
                        model.PageSizeOptions = model.PageSizeOptions.OrderBy(x => int.Parse(x.Value)).ToList();
                        model.AllowShippersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                            command.PageSize = int.Parse(model.PageSizeOptions.First().Value);
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = _dMSSettings.ShipmentPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = _dMSSettings.ShipmentPageSize;
            }

            return Task.CompletedTask;
        }

        protected async Task PrepareShippingStatusOptionsAsync(CourierShipmentsModel model, CourierShipmentsModel command)
        {
            switch ((ShippingStatus)command.ShippingStatusId)
            {
                case ShippingStatus.NotYetShipped:
                case ShippingStatus.Shipped:
                case ShippingStatus.Delivered:
                    model.ShippingStatusId = command.ShippingStatusId;
                    break;
                default:
                    model.ShippingStatusId = 0;
                    break;
            }

            model.AvailableShippingStatusOptions.Add(new SelectListItem()
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("NopStation.DMS.Shipments.List.ShippingStatusId.All"),
                Selected = 0 == model.ShippingStatusId,
            });
            model.AvailableShippingStatusOptions.Add(new SelectListItem()
            {
                Value = ((int)ShippingStatus.NotYetShipped).ToString(),
                Text = await _localizationService.GetLocalizedEnumAsync(ShippingStatus.NotYetShipped),
                Selected = (int)ShippingStatus.NotYetShipped == model.ShippingStatusId,
            });
            model.AvailableShippingStatusOptions.Add(new SelectListItem()
            {
                Value = ((int)ShippingStatus.Shipped).ToString(),
                Text = await _localizationService.GetLocalizedEnumAsync(ShippingStatus.Shipped),
                Selected = (int)ShippingStatus.Shipped == model.ShippingStatusId,
            });
            model.AvailableShippingStatusOptions.Add(new SelectListItem()
            {
                Value = ((int)ShippingStatus.Delivered).ToString(),
                Text = await _localizationService.GetLocalizedEnumAsync(ShippingStatus.Delivered),
                Selected = (int)ShippingStatus.Delivered == model.ShippingStatusId,
            });
        }

        protected async Task PrepareFilterOptionsAsync(CourierShipmentsModel model, CourierShipmentsModel command)
        {
            var availableFilterOptions = (await FilterOption.All.ToSelectListAsync()).ToList();

            foreach (var option in availableFilterOptions)
            {
                model.AvailableFilterOptions.Add(new SelectListItem
                {
                    Text = option.Text,
                    Selected = int.Parse(option.Value) == command.FilterOptionId,
                    Value = option.Value,
                });
            }

            switch ((FilterOption)command.FilterOptionId)
            {
                case FilterOption.LastWeek:
                    model.CreatedOnUtc = DateTime.UtcNow.AddDays(-7);
                    break;
                case FilterOption.LastMonth:
                    model.CreatedOnUtc = DateTime.UtcNow.AddMonths(-1);
                    break;
                case FilterOption.LastYear:
                    model.CreatedOnUtc = DateTime.UtcNow.AddYears(-1);
                    break;
                case FilterOption.All:
                default:
                    break;
            }
        }

        #endregion

        #region Methods

        public async Task<CourierShipmentsModel> PrepareCourierShipmentsOverviewModelAsync(CourierShipmentsModel model, Shipper shipper, CourierShipmentsModel command, ShipmentSearchModel searchModel)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (shipper == null)
                throw new ArgumentNullException(nameof(shipper));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            //page size
            await PreparePageSizeOptionsAsync(model, command);
            //shipping statuses
            await PrepareShippingStatusOptionsAsync(model, command);
            //filter options
            await PrepareFilterOptionsAsync(model, command);

            var shipments = await _courierShipmentService.GetAllCourierShipmentsAsync(
                createdOnUtc: model.CreatedOnUtc,
                courierShipmentStatusId: command.CourierShipmentStatusId,
                shipperId: shipper.Id,
                pageIndex: command.PageIndex,
                pageSize: command.PageSize,
                trackingNumber: command.TrackingNumber,
                email: command.Email,
                statusId: command.ShippingStatusId,
                shipmentId: searchModel.ShipmentId,
                orderId: searchModel.OrderId);

            foreach (var courierShipment in shipments)
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(courierShipment.ShipmentId);

                if (shipment == null)
                    throw new ArgumentNullException(nameof(shipment));

                var shippingStatus = shipment.DeliveryDateUtc.HasValue ? ShippingStatus.Delivered :
                    shipment.ShippedDateUtc.HasValue ? ShippingStatus.Shipped : ShippingStatus.NotYetShipped;

                var customerOrder = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                var shipmentOverviewModel = new CourierShipmentOverviewModel
                {
                    Id = courierShipment.Id,
                    ShipmentId = shipment.Id,
                    TrackingNumber = shipment.TrackingNumber,
                    CustomOrderNumber = customerOrder.CustomOrderNumber,
                    UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(courierShipment.UpdatedOnUtc, DateTimeKind.Utc),
                    PickedUpOn = shipment.ShippedDateUtc.HasValue ? await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc) : null,
                    DeliveredOn = shipment.DeliveryDateUtc.HasValue ? await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc) : null,
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(shippingStatus),
                    CourierShipmentStatusId = courierShipment.ShipmentStatusId,
                    CourierShipmentStatus = await _localizationService.GetLocalizedEnumAsync(courierShipment.ShipmentStatusType),
                };

                var deliverFailedRecord = (await _deliverFailedRecordService.GetAllDeliverFailedRecordByShipmentIdAsync(shipment.Id)).FirstOrDefault();
                if (deliverFailedRecord != null)
                {
                    shipmentOverviewModel.DeliverFailedReasonId = deliverFailedRecord.DeliverFailedReasonId;
                    shipmentOverviewModel.DeliverFailedReasonString = deliverFailedRecord.DeliverFailedReasonType.ToString();
                    shipmentOverviewModel.Note = deliverFailedRecord.Note;
                }
                if (courierShipment.ShipmentPickupPointId > 0)
                    await PrepareShipmentPickupPointInfoAsync(shipmentOverviewModel, courierShipment.ShipmentPickupPointId);

                var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
                var deliveryAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? order.PickupAddressId ?? order.BillingAddressId);
                if (deliveryAddress != null)
                {
                    var deliveryAddressStr = await PrepareAddressStringAsync(deliveryAddress);
                    if (!string.IsNullOrEmpty(deliveryAddressStr))
                    {
                        shipmentOverviewModel.DeliveryAddress = deliveryAddressStr;
                        shipmentOverviewModel.DeliveryAddressId = deliveryAddress.Id;
                    }

                    shipmentOverviewModel.ShipToName = deliveryAddress.FirstName + " " + deliveryAddress.LastName;
                }

                if (shipmentOverviewModel.DeliveredOn.HasValue)
                    shipmentOverviewModel.ReceiverName = $"{deliveryAddress.FirstName} {deliveryAddress.LastName}";

                if (string.IsNullOrWhiteSpace(shipmentOverviewModel.ShipToName))
                {
                    var customerName = await _customerService.GetCustomerFullNameAsync(await _customerService.GetCustomerByIdAsync(customerOrder.CustomerId));
                    shipmentOverviewModel.ShipToName = customerName;
                }

                model.CourierShipments.Add(shipmentOverviewModel);
            }

            model.LoadPagedList(shipments);

            return model;
        }

        public async Task<CourierShipmentDetailsModel> PrepareShipmentDetailsModelAsync(Shipment shipment, CourierShipment courierShipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            if (courierShipment == null)
                throw new ArgumentNullException(nameof(courierShipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            if (order == null)
                throw new Exception("order cannot be loaded");

            var model = new CourierShipmentDetailsModel
            {
                ShipmentId = shipment.Id,
                TrackingNumber = shipment.TrackingNumber,
                TotalWeight = shipment.TotalWeight,
                OrderId = shipment.OrderId,
                DeliveryDate = shipment.DeliveryDateUtc.HasValue ? await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc) : null,
                ShippedDate = shipment.ShippedDateUtc.HasValue ? await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc) : null,
                CanShip = !shipment.ShippedDateUtc.HasValue,
                CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue,
                CourierShipmentStatusId = courierShipment.ShipmentStatusId,
                CourierShipmentStatus = await _localizationService.GetLocalizedEnumAsync(courierShipment.ShipmentStatusType),
                PickupTime = shipment.ShippedDateUtc == null ? await _localizationService.GetResourceAsync("NopStation.DMS.Shipper.PickupTime.Default") : (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc)).ToString(),
            };

            if (order.ShippingAddressId.HasValue)
            {
                var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                var shippingCountry = await _countryService.GetCountryByAddressAsync(shippingAddress);
                await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress, shippingAddress, false, _addressSettings);

                model.ShippingAddressGoogleMapsUrl = "https://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q=" +
                    $"{WebUtility.UrlEncode(shippingAddress.Address1 + " " + shippingAddress.ZipPostalCode + " " + shippingAddress.City + " " + (shippingCountry?.Name ?? string.Empty))}";
            }

            if (courierShipment.ShipmentPickupPointId > 0)
            {
                var shipmentPickupPoint = await _shipmentPickupPointService.GetShipmentPickupPointByIdAsync(courierShipment.ShipmentPickupPointId);
                var pickupPointModel = shipmentPickupPoint.ToModel<ShipmentPickupPointModel>();

                if (shipmentPickupPoint.AddressId > 0)
                {
                    var pickupAddress = await _addressService.GetAddressByIdAsync(shipmentPickupPoint.AddressId);
                    await _addressModelFactory.PrepareAddressModelAsync(pickupPointModel.Address, pickupAddress, false, _addressSettings);
                    var pickupPointInfo = await PrepareAddressStringAsync(pickupAddress);
                    model.PickupPointInfo = pickupPointInfo;
                }
                model.PickupPoint = pickupPointModel;
            }
            return model;
        }

        #endregion
    }
}
