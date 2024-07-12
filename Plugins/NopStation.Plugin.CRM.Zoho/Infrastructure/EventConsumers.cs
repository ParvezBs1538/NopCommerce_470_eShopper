using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Services;

namespace NopStation.Plugin.CRM.Zoho.Infrastructure
{
    public class EventConsumers : IConsumer<EntityInsertedEvent<Store>>,
        IConsumer<EntityUpdatedEvent<Store>>,
        IConsumer<EntityInsertedEvent<Vendor>>,
        IConsumer<EntityUpdatedEvent<Vendor>>,
        IConsumer<EntityInsertedEvent<Customer>>,
        IConsumer<EntityUpdatedEvent<Customer>>,
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityInsertedEvent<Order>>,
        IConsumer<EntityUpdatedEvent<Order>>,
        IConsumer<EntityInsertedEvent<Shipment>>,
        IConsumer<EntityUpdatedEvent<Shipment>>,
        IConsumer<EntityInsertedEvent<ShipmentItem>>,
        IConsumer<EntityUpdatedEvent<ShipmentItem>>
    {
        private readonly IUpdatableItemService _updatableItemService;

        public EventConsumers(IUpdatableItemService updatableItemService)
        {
            _updatableItemService = updatableItemService;
        }

        protected async Task InsertUpdatableItemAsync(EntityType entityType, BaseEntity baseEntity, bool checkExists = false)
        {
            if (checkExists && await _updatableItemService.GetUpdatableItemByEntityTypeAndIdAsync(entityType, baseEntity.Id) != null)
                return;

            await _updatableItemService.InsertUpdatableItemAsync(new UpdatableItem
            {
                EntityId = baseEntity.Id,
                EntityTypeId = (int)entityType
            });
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Store> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Stores, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Store> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Stores, eventMessage.Entity, true);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Vendor> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Vendors, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Vendors, eventMessage.Entity, true);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Customer> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Customers, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Customer> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Customers, eventMessage.Entity, true);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Products, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Products, eventMessage.Entity, true);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Orders, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Orders, eventMessage.Entity, true);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Shipment> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Shipments, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Shipment> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.Shipments, eventMessage.Entity, true);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ShipmentItem> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.ShipmentItems, eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ShipmentItem> eventMessage)
        {
            await InsertUpdatableItemAsync(EntityType.ShipmentItems, eventMessage.Entity, true);
        }
    }
}
