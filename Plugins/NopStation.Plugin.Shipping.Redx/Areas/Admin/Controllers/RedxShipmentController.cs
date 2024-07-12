using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;
using NopStation.Plugin.Shipping.Redx.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Shipping.Redx.Models;
using System.Collections.Generic;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Controllers
{
    public class RedxShipmentController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly RedxSettings _redxSettings;
        private readonly IRedxShipmentService _redxShipmentService;
        private readonly IRedxAreaService _redxAreaService;
        private readonly IRedxShipmentModelFactory _redxShipmentModelFactory;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IAddressService _addressService;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public RedxShipmentController(IPermissionService permissionService,
            ILocalizationService localizationService,
            RedxSettings redxSettings,
            IRedxShipmentService redxShipmentService,
            IRedxAreaService redxAreaService,
            IRedxShipmentModelFactory redxShipmentModelFactory,
            IOrderService orderService,
            ILogger logger,
            IAddressService addressService,
            IProductService productService,
            IShipmentService shipmentService,
            ICategoryService categoryService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _redxSettings = redxSettings;
            _redxShipmentService = redxShipmentService;
            _redxAreaService = redxAreaService;
            _redxShipmentModelFactory = redxShipmentModelFactory;
            _orderService = orderService;
            _logger = logger;
            _addressService = addressService;
            _productService = productService;
            _shipmentService = shipmentService;
            _categoryService = categoryService;
        }

        #endregion

        #region Utilities

        protected async Task<SendPercelRequestModel> GenerateParcelInsertRequest(Shipment shipment, RedxArea redxArea)
        {
            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            var address = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);

            var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            var orderItemWeight = orderItems.Sum(item => item.ItemWeight);

            var parcel = new SendPercelRequestModel
            {
                CustomerName = address.FirstName + " " + address.LastName,
                CustomerPhone = address.PhoneNumber,
                DeliveryArea = redxArea.Name,
                DeliveryAreaId = redxArea.RedxAreaId,
                MerchantInvoiceId = order.CustomOrderNumber,
                CustomerAddress = address.Address1.Replace("'", ""),
                CashCollectionAmount = (order.PaymentStatus == PaymentStatus.Paid ? 0 : order.OrderTotal).ToString(),
                ParcelWeight = (int)((orderItemWeight > 0 ? orderItemWeight : 0) ?? 0 * 1000)
            };

            double value = 0;
            foreach (var item in orderItems)
            {
                var shipmentItem = shipmentItems.FirstOrDefault(x => x.OrderItemId == item.Id);
                if (shipmentItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(item.ProductId);

                var pcd = new ParcelDetails()
                {
                    Name = product.Name,
                    Value = Convert.ToDouble(item.PriceInclTax / item.Quantity *  shipmentItem.Quantity)
                };

                var productCategory = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).FirstOrDefault();
                if (productCategory != null)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
                    pcd.Category = category.Name;
                }

                value += pcd.Value;
                parcel.ParcelDetails.Add(pcd);
            }

            parcel.Value = Convert.ToInt32(value);
            return parcel;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _redxShipmentModelFactory.PrepareRedxShipmentSearchModelAsync(new RedxShipmentSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(RedxShipmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _redxShipmentModelFactory.PrepareRedxShipmentListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> Details(int shipmentId)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId)
                ?? throw new ArgumentNullException("No shipemnt found with the specific id");

            var redxShipment = await _redxShipmentService.GetRedxShipmentByShipmentIdAsync(shipmentId);
            var model = redxShipment == null ? new RedxShipmentModel() : null;
            model = await _redxShipmentModelFactory.PrepareRedxShipmentModelAsync(model, redxShipment, shipment);

            var html = await RenderPartialViewToStringAsync("Details", model);

            return Json(new
            {
                success = true,
                html
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendShipmentRequest(int shipmentId, int redxAreaId)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                throw new ArgumentNullException();

            var existingRedxShipment = await _redxShipmentService.GetRedxShipmentByShipmentIdAsync(shipmentId);
            if (existingRedxShipment != null)
                return Json(new
                {
                    success = false,
                    message = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.ParcelAlreadyCreated")
                });

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order.OrderStatusId == (int)OrderStatus.Cancelled)
                return Json(new
                {
                    success = false,
                    message = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.CancelOrder")
                });

            var redxArea = await _redxAreaService.GetRedxAreaByRedxAreaIdAsync(redxAreaId);

            var requestBody = await GenerateParcelInsertRequest(shipment, redxArea);

            var headers = new Dictionary<string, string>();
            headers.Add("Content-type", "application/json");
            headers.Add("API-ACCESS-TOKEN", "Bearer " + _redxSettings.ApiAccessToken);

            var responseModel = _redxSettings.GetBaseUri().Concat("parcel").Post<SendPercelResponseModel>(headers, model: requestBody);

            if (responseModel.Success)
            {
                if (responseModel.Model.TrackingId != null)
                {
                    var redxShipment = new RedxShipment()
                    {
                        TrackingId = responseModel.Model.TrackingId,
                        ShipmentId = shipmentId,
                        RedxAreaId = redxAreaId,
                        OrderId = shipment.OrderId,
                    };
                    await _redxShipmentService.InsertShipmentAsync(redxShipment);

                    shipment.TrackingNumber = responseModel.Model.TrackingId;
                    await _shipmentService.UpdateShipmentAsync(shipment);

                    var model = await _redxShipmentModelFactory.PrepareRedxShipmentModelAsync(null, redxShipment, shipment);
                    var html = await RenderPartialViewToStringAsync("Details", model);

                    return Json(new
                    {
                        success = true,
                        html
                    });
                }
                await _logger.ErrorAsync("Redx Shipment Failed: " + responseModel.Model.Message);
                
                return Json(new
                {
                    success = false,
                    message = responseModel.Model.Message
                });
            }

            return Json(new
            {
                success = false,
                message = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.RedxShipment.CreateFailed")
            });
        }

        #endregion
    }
}
