using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Models;
using NopStation.Plugin.Shipping.DHL.Domain;
using NopStation.Plugin.Shipping.DHL.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Factories
{
    public class DHLModelFactory : IDHLModelFactory
    {

        private readonly IDHLAcceptedServicesService _dhlAcceptedServicesService;
        private readonly IDHLOrderService _dhlOrderService;
        private readonly IStoreService _storeService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDHLShipmentService _dhlShipmentService;
        private readonly IDHLPickupRequestService _dHLPickupRequestService;

        public DHLModelFactory(IDHLAcceptedServicesService dhlAcceptedServicesService,
            IDHLOrderService dhlOrderService,
            IStoreService storeService,
            IPriceFormatter priceFormatter,
            IAddressService addressService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IDHLShipmentService dhlShipmentService,
            IDHLPickupRequestService dHLPickupRequestService)
        {
            _dhlAcceptedServicesService = dhlAcceptedServicesService;
            _dhlOrderService = dhlOrderService;
            _storeService = storeService;
            _priceFormatter = priceFormatter;
            _addressService = addressService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _dhlShipmentService = dhlShipmentService;
            _dHLPickupRequestService = dHLPickupRequestService;
        }

        public virtual async Task<DHLServiceListModel> PrepareDHLServiceListModelAsync(DHLServiceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var services = await _dhlAcceptedServicesService.GetAllDHLServicesAsync(searchModel.Page - 1, searchModel.PageSize);

            var model = new DHLServiceListModel().PrepareToGrid(searchModel, services, () =>
            {
                return services.Select(service =>
                {
                    //fill in model values from the entity
                    var dhlServiceModel = new DHLServiceModel()
                    {
                        Id = service.Id,
                        GlobalProductCode = service.GlobalProductCode,
                        IsActive = service.IsActive,
                        ServiceName = service.Name
                    };

                    return dhlServiceModel;
                });
            });

            return model;
        }


        public virtual async Task<DHLOrderListModel> PrepareDHLOrderListModelAsync(DHLOrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var orders = await _dhlOrderService.SearchOrderAsync(searchModel.Page - 1, searchModel.PageSize, searchModel.OrderId);

            var model = await new DHLOrderListModel().PrepareToGridAsync(searchModel, orders, () =>
            {
                return orders.SelectAwait(async order =>
                {
                    //fill in model values from the entity
                    var store = await _storeService.GetStoreByIdAsync(order.StoreId);

                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId)
                        ?? throw new ArgumentException("No address found with the specified id", nameof(order.BillingAddressId));

                    var dhlOrderModel = new DHLOrderModel()
                    {
                        Id = order.Id,
                        StoreName = store != null ? store.Name : "Unknown",
                        OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false),
                        OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                        OrderStatusId = order.OrderStatusId,
                        PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                        PaymentStatusId = order.PaymentStatusId,
                        ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                        ShippingStatusId = order.ShippingStatusId,
                        CustomerEmail = billingAddress.Email,
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                        CustomOrderNumber = order.CustomOrderNumber
                    };

                    var dhlshipment = _dhlShipmentService.GetDHLShipmentSubmissionByOrderId(order.Id);
                    if (dhlshipment != null)
                    {
                        dhlOrderModel.HasShippingLabel = !string.IsNullOrEmpty(dhlshipment.ShippingLabelBase64Pdf);
                        dhlOrderModel.AirwayBillNumber = dhlshipment.AirwayBillNumber;
                    }

                    var bookedPickupRequest = _dHLPickupRequestService.GetDHLPickupRequestByOrderId(order.Id);
                    if (bookedPickupRequest != null)
                    {
                        dhlOrderModel.ConfirmationNumber = bookedPickupRequest.ConfirmationNumber;
                        dhlOrderModel.ReadyByTime = bookedPickupRequest.ReadyByTime;
                        dhlOrderModel.NextPickupDate = bookedPickupRequest.NextPickupDate;
                    }
                    else
                        dhlOrderModel.CanBookPickup = !string.IsNullOrEmpty(dhlOrderModel.AirwayBillNumber);

                    return dhlOrderModel;
                });
            });

            return model;
        }
    }
}
