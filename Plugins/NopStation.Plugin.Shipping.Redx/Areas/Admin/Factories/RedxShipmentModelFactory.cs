using System;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;
using NopStation.Plugin.Shipping.Redx.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Models.Extensions;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories
{
    public class RedxShipmentModelFactory : IRedxShipmentModelFactory
    {
        private readonly IRedxShipmentService _redxShipmentService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IShipmentService _shipmentService;
        private readonly IRedxAreaService _redxAreaService;
        private readonly IAddressService _addressService;

        public RedxShipmentModelFactory(IOrderService orderService,
            IRedxShipmentService redxShipmentService,
            ILocalizationService localizationService,
            IShipmentService shipmentService,
            IRedxAreaService redxAreaService,
            IAddressService addressService)
        {
            _redxShipmentService = redxShipmentService;
            _orderService = orderService;
            _localizationService = localizationService;
            _shipmentService = shipmentService;
            _redxAreaService = redxAreaService;
            _addressService = addressService;
        }

        public Task<RedxShipmentSearchModel> PrepareRedxShipmentSearchModelAsync(RedxShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            return Task.FromResult(searchModel);
        }

        public async Task<RedxShipmentModel> PrepareRedxShipmentModelAsync(RedxShipmentModel model, RedxShipment redxShipment, 
            Shipment shipment, bool excludeProperties = false)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (redxShipment != null)
            {
                if (model == null)
                {
                    model = redxShipment.ToModel<RedxShipmentModel>();
                    model.OrderId = shipment.OrderId;
                    model.CustomOrderNumber = order.CustomOrderNumber;
                    model.OrderStatusId = order.OrderStatusId;
                    model.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);

                    var area = await _redxAreaService.GetRedxAreaByRedxAreaIdAsync(redxShipment.RedxAreaId);
                    model.RedxAreaName = area.Name;
                }
            }

            model.CanSendShipmentToRedx = redxShipment == null;
            model.ShipmentId = shipment.Id;

            if (!excludeProperties)
            {
                var address = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
                var areas = await _redxAreaService.GetRedxAreasAsync(address.StateProvinceId);

                foreach (var area in areas)
                {
                    model.AvailableRedxAreas.Add(new SelectListItem
                    {
                        Text = area.Name,
                        Value = area.RedxAreaId.ToString(),
                    });
                }
            }

            return model;
        }

        public async Task<RedxShipmentListModel> PrepareRedxShipmentListModelAsync(RedxShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var redxShipments = await _redxShipmentService.GetAllRedxShipmentsAsync(
                orderNumber: searchModel.SearchOrderId,
                trackingId: searchModel.SearchTrackingId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new RedxShipmentListModel().PrepareToGridAsync(searchModel, redxShipments, () =>
            {
                return redxShipments.SelectAwait(async redxShipment =>
                {
                    var shipment = await _shipmentService.GetShipmentByIdAsync(redxShipment.ShipmentId);
                    return await PrepareRedxShipmentModelAsync(null, redxShipment, shipment, true);
                });
            });

            return model;
        }
    }
}