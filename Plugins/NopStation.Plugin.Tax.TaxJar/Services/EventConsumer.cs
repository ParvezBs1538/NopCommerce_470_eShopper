using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public class EventConsumer :
        IConsumer<EntityDeletedEvent<Order>>,
        IConsumer<ModelReceivedEvent<BaseNopModel>>,
        IConsumer<OrderStatusChangedEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderRefundedEvent>,
        IConsumer<OrderVoidedEvent>
    {
        #region Fields

        private readonly TaxJarManager _taxJarManager;
        private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ITaxPluginManager _taxPluginManager;
        private readonly TaxJarSettings _taxJarSettings;

        #endregion

        #region Ctor

        public EventConsumer(TaxJarManager taxJarManager,
            IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
            ICustomerService customerService,
            IPermissionService permissionService,
            IProductService productService,
            ITaxPluginManager taxPluginManager,
            TaxJarSettings taxJarSettings)
        {
            _taxJarManager = taxJarManager;
            _checkoutAttributeService = checkoutAttributeService;
            _customerService = customerService;
            _permissionService = permissionService;
            _productService = productService;
            _taxPluginManager = taxPluginManager;
            _taxJarSettings = taxJarSettings;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(ModelReceivedEvent<BaseNopModel> eventMessage)
        {
            //get entity by received model
            var entity = eventMessage.Model switch
            {
                CustomerModel customerModel => (BaseEntity)await _customerService.GetCustomerByIdAsync(customerModel.Id),
                CustomerRoleModel customerRoleModel => await _customerService.GetCustomerRoleByIdAsync(customerRoleModel.Id),
                ProductModel productModel => await _productService.GetProductByIdAsync(productModel.Id),
                CheckoutAttributeModel checkoutAttributeModel => await _checkoutAttributeService.GetAttributeByIdAsync(checkoutAttributeModel.Id),
                _ => null
            };
            if (entity == null)
                return;

            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
                return;

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
                return;
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Order.CustomerId);
            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName, customer, eventMessage.Order.StoreId))
                return;

            if (_taxJarSettings.DisableTaxSubmit)
                return;
            //create tax transaction
            await _taxJarManager.CreateUpdateOrderTaxTransactionAsync(eventMessage.Order, false);

        }

        public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
                return;

            if (_taxJarSettings.DisableTaxSubmit)
                return;

            await _taxJarManager.CreateUpdateRefundTaxTransactionAsync(eventMessage.Order, eventMessage.Amount, false);
        }

        public async Task HandleEventAsync(OrderVoidedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
                return;

            if (_taxJarSettings.DisableTaxSubmit)
                return;
            //void tax transaction
            await _taxJarManager.DeleteOrderTaxTransactionAsync(eventMessage.Order);
            await _taxJarManager.DeleteRefundTransactionAsync(eventMessage.Order);
        }

        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            if (eventMessage.Order.OrderStatus != OrderStatus.Cancelled)
                return;

            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
                return;

            if (_taxJarSettings.DisableTaxSubmit)
                return;

            //void tax transaction
            await _taxJarManager.DeleteOrderTaxTransactionAsync(eventMessage.Order);
            await _taxJarManager.DeleteRefundTransactionAsync(eventMessage.Order);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
        {
            if (eventMessage.Entity == null)
                return;

            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
                return;

            if (_taxJarSettings.DisableTaxSubmit)
                return;

            //delete tax transaction
            await _taxJarManager.DeleteOrderTaxTransactionAsync(eventMessage.Entity);
            await _taxJarManager.DeleteRefundTransactionAsync(eventMessage.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            if (eventMessage.Entity == null)
                return;

            if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
                return;

            if (_taxJarSettings.DisableTaxSubmit)
                return;

            //delete tax transaction
            await _taxJarManager.CreateUpdateOrderTaxTransactionAsync(eventMessage.Entity, true);
        }

        #endregion
    }
}
