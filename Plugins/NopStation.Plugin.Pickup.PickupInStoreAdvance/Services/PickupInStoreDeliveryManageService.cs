using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using System.IO;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Services
{
    public class PickupInStoreDeliveryManageService : IPickupInStoreDeliveryManageService
    {
        #region Fields

        private readonly IRepository<PickupInStoreDeliveryManage> _repository;
        private readonly IPickupInStoreMessageService _pickupInStoreMessageService;
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IShipmentService _shipmentService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region CTOR
        public PickupInStoreDeliveryManageService(IRepository<PickupInStoreDeliveryManage> repository,
            IPickupInStoreMessageService pickupInStoreMessageService,
            IOrderService orderService,
            IPdfService pdfService,
            OrderSettings orderSettings,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IShipmentService shipmentService,
            ICustomerActivityService customerActivityService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _repository = repository;
            _pickupInStoreMessageService = pickupInStoreMessageService;
            _orderService = orderService;
            _pdfService = pdfService;
            _orderSettings = orderSettings;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _shipmentService = shipmentService;
            _customerActivityService = customerActivityService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilities
        protected virtual async Task LogEditOrderAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            await _customerActivityService.InsertActivityAsync("EditOrder",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }
        protected async Task CreateShipmentAsync(Order order)
        {

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true);

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = string.Empty,
                TotalWeight = null,
                AdminComment = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Shipment.AdminComment"),
                CreatedOnUtc = DateTime.UtcNow
            };

            var shipmentItems = new List<ShipmentItem>();

            decimal? totalWeight = null;

            foreach (var orderItem in orderItems)
            {
                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight * orderItem.Quantity;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                //create a shipment item
                shipmentItems.Add(new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = orderItem.Quantity,
                    WarehouseId = 0
                });

            }

            //if we have at least one item in the shipment, then save it
            if (shipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                await _shipmentService.InsertShipmentAsync(shipment);

                foreach (var shipmentItem in shipmentItems)
                {
                    shipmentItem.ShipmentId = shipment.Id;
                    await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                }

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                await _orderProcessingService.ShipAsync(shipment, notifyCustomer: false);
                //_orderProcessingService.Deliver(shipment, true);
                await LogEditOrderAsync(order.Id);
            }

        }

        protected async Task SendOrderReadyEmailAndInsertOrderNoteAsync(Order order)
        {
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;
            var settings = await _settingService.LoadSettingAsync<PickupInStoreSettings>(storeScope);
            var orderReadyCustomerNotificationQueuedEmailIds = new List<int>();
            if (settings.NotifyCustomerIfReady)
            {
                var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    await _pdfService.SaveOrderPdfToDiskAsync(order) : null;
                var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;
                orderReadyCustomerNotificationQueuedEmailIds.AddRange(await _pickupInStoreMessageService.SendOrderReadyCustomerNotificationAsync(order, order.CustomerLanguageId,
                        orderPaidAttachmentFilePath, orderPaidAttachmentFileName));
            }

            if (orderReadyCustomerNotificationQueuedEmailIds.Any() && settings.AddOrderNote)
            {
                await _orderService.InsertOrderNoteAsync(
                    new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"\"Order is ready to collect\".",// Queued email identifiers: {string.Join(", ", orderReadyCustomerNotificationQueuedEmailIds)}.",
                        DisplayToCustomer = settings.OrderNotesShowToCustomer,
                        CreatedOnUtc = DateTime.UtcNow
                    });
            }
        }

        #endregion

        #region Methods

        public async Task DeleteAsync(PickupInStoreDeliveryManage pickupInStoreDeliveryManage)
        {
            await _repository.DeleteAsync(pickupInStoreDeliveryManage);
        }

        public Task<List<PickupInStoreDeliveryManage>> GetAllOrdersAsync(PickupInStoreDeliveryManageSearchModel searchModel)
        {
            var query = _repository.Table;
            if (!string.IsNullOrEmpty(searchModel.SearchOrderId))
            {
                int orderId = 0;
                if (int.TryParse(searchModel.SearchOrderId, out orderId))
                {
                    query = query.Where(x => x.OrderId == orderId);
                }
            }
            if (!string.IsNullOrEmpty(searchModel.SearchStatusId))
            {
                int statusId = 0;
                if (int.TryParse(searchModel.SearchStatusId, out statusId) && statusId != 0)
                {
                    query = query.Where(x => x.PickUpStatusTypeId == statusId);
                }
            }
            return Task.FromResult(query.OrderByDescending(x => x.OrderId).ToList());
        }

        public Task<PickupInStoreDeliveryManage> GetPickupInStoreDeliverManageByOrderIdAsync(int orderId)
        {
            var query = _repository.Table.Where(x => x.OrderId == orderId);
            var shipment = query.FirstOrDefault();
            return Task.FromResult(shipment);
        }

        public async Task InsertAsync(PickupInStoreDeliveryManage pickupInStoreDeliveryManage)
        {
            await _repository.InsertAsync(pickupInStoreDeliveryManage);
        }

        public async Task UpdateAsync(PickupInStoreDeliveryManage pickupInStoreDeliveryManage)
        {
            await _repository.UpdateAsync(pickupInStoreDeliveryManage);
        }

        public async Task<bool> MarkAsReadyToPickupAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }
            var deliveryManage = await GetPickupInStoreDeliverManageByOrderIdAsync(orderId);
            if (deliveryManage == null)
                return false;
            deliveryManage.PickUpStatusType = Domain.Enum.PickUpStatusType.ReadyForPick;
            deliveryManage.ReadyForPickupMarkedAtUtc = DateTime.UtcNow;
            await UpdateAsync(deliveryManage);
            await SendOrderReadyEmailAndInsertOrderNoteAsync(order);
            await CreateShipmentAsync(order);
            return true;
        }
        public async Task<bool> MarkAsPickedByCustomerAsync(int orderId)
        {
            var deliveryManage = await GetPickupInStoreDeliverManageByOrderIdAsync(orderId);
            if (deliveryManage == null)
                return false;
            deliveryManage.PickUpStatusType = Domain.Enum.PickUpStatusType.PickedUpByCustomer;
            deliveryManage.CustomerPickedUpAtUtc = DateTime.UtcNow;
            await UpdateAsync(deliveryManage);
            var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(orderId);
            if (shipments.Any())
            {
                foreach (var shipment in shipments)
                {
                    if (!shipment.DeliveryDateUtc.HasValue)
                        await _orderProcessingService.DeliverAsync(shipment, true);
                }
            }
            await LogEditOrderAsync(orderId);
            return true;
        }

        #endregion
    }


}
