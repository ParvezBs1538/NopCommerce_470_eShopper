using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Tax;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    /// <summary>
    /// Represents event consumer
    /// </summary>
    public class EventConsumer : IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
                                IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
                                IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
                                IConsumer<EntityInsertedEvent<OrderItem>>
    {
        #region Fields

        private readonly IAbandonedCartService _abandonedCartService;
        private readonly ICustomerAbandonmentInfoService _customerAbandonmentInfoService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        private readonly ITaxService _taxService;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctors

        public EventConsumer(IAbandonedCartService abandonedCartService,
                            ISettingService settingService,
                            IStoreContext storeContext,
                            ICustomerAbandonmentInfoService customerAbandonmentInfoService,
                            IOrderService orderService,
                            IPriceFormatter priceFormatter,
                            IProductAttributeFormatter productAttributeFormatter,
                            IProductService productService,
                            ITaxService taxService,
                            IShoppingCartService shoppingCartService,
                            ICustomerService customerService)
        {
            _abandonedCartService = abandonedCartService;
            _customerAbandonmentInfoService = customerAbandonmentInfoService;
            _settingService = settingService;
            _storeContext = storeContext;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
            _taxService = taxService;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            //handle event
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            if (!abandonedCartsSettings.IsEnabled)
                return;

            var cartItem = eventMessage.Entity;
            if (cartItem == null)
                return;
            var customer = new Customer
            {
                Id = cartItem.CustomerId
            };
            if (!await _customerService.IsRegisteredAsync(customer))
                return;
            var abandonedCart = new AbandonedCartModel();
            abandonedCart.CustomerId = cartItem.CustomerId;
            abandonedCart.ShoppingCartItemId = cartItem.Id;
            abandonedCart.ProductId = cartItem.ProductId;
            abandonedCart.StatusId = (int)AbandonedStatus.InAction;
            abandonedCart.StatusChangedOn = DateTime.UtcNow;
            abandonedCart.IsSoftDeleted = false;
            abandonedCart.Quantity = cartItem.Quantity;

            var item = (await _shoppingCartService.GetShoppingCartAsync(customer: customer, productId: abandonedCart.ProductId)).FirstOrDefault();
            var product = await _productService.GetProductByIdAsync(abandonedCart.ProductId);
            if (item != null)
            {
                abandonedCart.AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, item.AttributesXml, customer, _storeContext.GetCurrentStore());
                var (unitPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(item, true);
                abandonedCart.UnitPrice = await _priceFormatter.FormatPriceAsync((await _taxService.GetProductPriceAsync(product, unitPrice)).price);
                var (subTotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(item, true);
                abandonedCart.TotalPrice = await _priceFormatter.FormatPriceAsync((await _taxService.GetProductPriceAsync(product, subTotal)).price);
            }

            await _abandonedCartService.AddOrUpdateAbandonedCartAsync(abandonedCart);

            var customerAbandonment = await _customerAbandonmentInfoService.GetCustomerAbandonmentByCustomerIdAsync(cartItem.CustomerId);
            if (customerAbandonment != null)
                return;
            customerAbandonment = new CustomerAbandonmentInfoModel();
            customerAbandonment.NotificationSentFrequency = 0;
            customerAbandonment.CustomerId = cartItem.CustomerId;
            customerAbandonment.StatusId = (int)CustomerAbandonmentStatus.Subscribed;

            await _customerAbandonmentInfoService.AddOrUpdateCustomerAbandonmentAsync(customerAbandonment);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            //handle event
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            if (!abandonedCartsSettings.IsEnabled)
                return;

            var cartItem = eventMessage.Entity;
            if (cartItem == null)
                return;
            var customer = new Customer
            {
                Id = cartItem.CustomerId
            };
            if (!await _customerService.IsRegisteredAsync(customer))
                return;

            var abandonedCart = new AbandonedCartModel();
            abandonedCart.CustomerId = cartItem.CustomerId;
            abandonedCart.ShoppingCartItemId = cartItem.Id;
            abandonedCart.ProductId = cartItem.ProductId;
            abandonedCart.StatusId = (int)AbandonedStatus.InAction;
            abandonedCart.StatusChangedOn = DateTime.UtcNow;
            abandonedCart.IsSoftDeleted = false;
            abandonedCart.Quantity = cartItem.Quantity;

            var item = (await _shoppingCartService.GetShoppingCartAsync(customer: customer, productId: abandonedCart.ProductId)).FirstOrDefault();
            var product = await _productService.GetProductByIdAsync(abandonedCart.ProductId);
            if (item != null)
            {
                abandonedCart.AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, item.AttributesXml, customer, _storeContext.GetCurrentStore());
                var (unitPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(item, true);
                abandonedCart.UnitPrice = await _priceFormatter.FormatPriceAsync((await _taxService.GetProductPriceAsync(product, unitPrice)).price);
                var (subTotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(item, true);
                abandonedCart.TotalPrice = await _priceFormatter.FormatPriceAsync((await _taxService.GetProductPriceAsync(product, subTotal)).price);
            }

            await _abandonedCartService.AddOrUpdateAbandonedCartAsync(abandonedCart);

            var customerAbandonment = await _customerAbandonmentInfoService.GetCustomerAbandonmentByCustomerIdAsync(cartItem.CustomerId);
            if (customerAbandonment != null)
                return;
            customerAbandonment = new CustomerAbandonmentInfoModel();
            customerAbandonment.NotificationSentFrequency = 0;
            customerAbandonment.CustomerId = cartItem.CustomerId;
            customerAbandonment.StatusId = (int)CustomerAbandonmentStatus.Subscribed;

            await _customerAbandonmentInfoService.AddOrUpdateCustomerAbandonmentAsync(customerAbandonment);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            //handle event
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            if (!abandonedCartsSettings.IsEnabled)
                return;

            var cartItem = eventMessage.Entity;
            var oldCart = await _abandonedCartService.GetAbandonedCartByShoppingCartIdAsync(cartItem.Id);
            if (oldCart == null || oldCart.StatusId == (int)AbandonedStatus.Recovered)
                return;

            var abandonedCart = new AbandonedCartModel();
            if (cartItem != null)
            {
                abandonedCart.CustomerId = cartItem.CustomerId;
                abandonedCart.ShoppingCartItemId = cartItem.Id;
                abandonedCart.ProductId = cartItem.ProductId;
                abandonedCart.StatusId = (int)AbandonedStatus.Deleted;

                abandonedCart.StatusChangedOn = DateTime.UtcNow;

                await _abandonedCartService.AddOrUpdateAbandonedCartAsync(abandonedCart);
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<OrderItem> eventMessage)
        {
            //handle event
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            if (!abandonedCartsSettings.IsEnabled)
                return;

            var cartItem = eventMessage.Entity;
            var order = await _orderService.GetOrderByIdAsync(cartItem.OrderId);
            var abandonedCart = new AbandonedCartModel();
            if (cartItem != null)
            {
                abandonedCart.CustomerId = order.CustomerId;
                abandonedCart.ProductId = cartItem.ProductId;
                abandonedCart.StatusId = (int)AbandonedStatus.Recovered;
                abandonedCart.StatusChangedOn = DateTime.UtcNow;
                abandonedCart.IsSoftDeleted = false;

                await _abandonedCartService.AddOrUpdateAbandonedCartAsync(abandonedCart);
            }
        }

        #endregion
    }
}
