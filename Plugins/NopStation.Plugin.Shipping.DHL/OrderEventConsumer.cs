using Nop.Core.Domain.Orders;
using NopStation.Plugin.Shipping.DHL.Domain;
using NopStation.Plugin.Shipping.DHL.Services;
using Nop.Services.Events;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL
{
    public class OrderEventConsumer : IConsumer<OrderPlacedEvent>
    {
        //private readonly IPluginFinder _pluginFinder;
        //private readonly IOrderService _orderService;
        //private readonly IStoreContext _storeContext;
        //private readonly IShoppingCartService _shoppingCartService;
        //private readonly IWorkContext _workContext;
        //private readonly IProductService _productService;
        //private readonly IDbContext _dbContext;
        private readonly IDHLAcceptedServicesService _dhlAcceptedServicesService;
        private readonly IDHLShipmentService _dhlShipmentSubmissionService;


        public OrderEventConsumer(
            //IPluginFinder pluginFinder,
            //IOrderService orderService,
            //IStoreContext storeContext,
            //IShoppingCartService shoppingCartService,
            //IWorkContext workContext,
            //IProductService productService,
            //IDbContext dbContext,
            IDHLAcceptedServicesService dhlAcceptedServicesService,
            IDHLShipmentService dhlShipmentSubmissionService)
        {
        //    this._pluginFinder = pluginFinder;
        //    this._orderService = orderService;
        //    this._storeContext = storeContext;
        //    _shoppingCartService = shoppingCartService;
        //    _workContext = workContext;
        //    _productService = productService;
        //    _dbContext = dbContext;
            _dhlAcceptedServicesService = dhlAcceptedServicesService;
            _dhlShipmentSubmissionService = dhlShipmentSubmissionService;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        ///    [HubMethodName("liveNotification")]
        ///    
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            var dhlChecker = eventMessage.Order.ShippingMethod.Contains("DHL");
            
            if (dhlChecker)
            {
                var dhlServiceName = eventMessage.Order.ShippingMethod;
                var globalProductCode = dhlServiceName.Split(' ').Last();

                var model = new DHLShipment
                {
                    GlobalProductCode = globalProductCode,
                    OrderId = eventMessage.Order.Id
                };

                await _dhlShipmentSubmissionService.InsertShipmentSubmissionAsync(model);
            }
        }
    }
}
