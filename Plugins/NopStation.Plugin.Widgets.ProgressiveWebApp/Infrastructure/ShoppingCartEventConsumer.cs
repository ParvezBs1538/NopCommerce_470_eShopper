using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure
{
    public class ShoppingCartEventConsumer : IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        IConsumer<EntityDeletedEvent<ShoppingCartItem>>
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IAbandonedCartTrackingService _abandonedCartTrackingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ShoppingCartEventConsumer(ICustomerService customerService,
            IAbandonedCartTrackingService abandonedCartTrackingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext)
        {
            _customerService = customerService;
            _abandonedCartTrackingService = abandonedCartTrackingService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilities

        public async Task RegisterCartUpdateAsync(Customer customer)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var tracking = await _abandonedCartTrackingService.GetAbandonedCartTrackingByCustomerAsync(customer.Id);
            if (cart.Count > 0)
            {
                if (tracking != null)
                {
                    tracking.LastModifiedOnUtc = DateTime.UtcNow;
                    await _abandonedCartTrackingService.UpdateAbandonedCartTrackingAsync(tracking);
                }
                else
                {
                    tracking = new AbandonedCartTracking()
                    {
                        CustomerId = customer.Id,
                        IsQueued = false,
                        LastModifiedOnUtc = DateTime.UtcNow
                    };
                    await _abandonedCartTrackingService.InsertAbandonedCartTrackingAsync(tracking);
                }
            }
            else
            {
                if (tracking != null)
                {
                    await _abandonedCartTrackingService.DeleteAbandonedCartTrackingAsync(tracking);
                }
            }
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
            if (!await _customerService.IsRegisteredAsync(customer))
                return;

            await RegisterCartUpdateAsync(customer);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
            if (!await _customerService.IsRegisteredAsync(customer))
                return;

            await RegisterCartUpdateAsync(customer);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
            if (!await _customerService.IsRegisteredAsync(customer))
                return;

            await RegisterCartUpdateAsync(customer);
        }

        #endregion
    }
}
