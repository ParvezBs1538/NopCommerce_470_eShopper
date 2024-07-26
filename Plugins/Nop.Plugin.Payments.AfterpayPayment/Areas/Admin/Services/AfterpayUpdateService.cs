using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Payments.AfterpayPayment.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Services
{
    public class AfterpayUpdateService : IAfterpayUpdateService
    {
        private readonly IOrderService _orderService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IAfterpayPaymentService _afterpayRequestService;

        public AfterpayUpdateService(IOrderService orderService,
            IRepository<Order> orderRepository,
            IStoreContext storeContext,
            ISettingService settingService,
            ILogger logger,
            IAfterpayPaymentService afterpayRequestService)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _storeContext = storeContext;
            _settingService = settingService;
            _logger = logger;
            _afterpayRequestService = afterpayRequestService;
        }
        public void UpdateOrderPaymentStatus()
        {
            var query = _orderRepository.Table;
            var orders = query.Where(x =>
                            x.PaymentMethodSystemName == AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME
                            && x.PaymentStatusId == (int)PaymentStatus.Pending && x.CreatedOnUtc.AddMinutes(30) >= DateTime.UtcNow)
                        .ToList();

            foreach (var order in orders)
            {
                var capturedResponse = _afterpayRequestService.GetCapturedResponse(order);
                if (capturedResponse.Status == AfterpayPaymentDefaults.APPROVED && capturedResponse.PaymentState == AfterpayPaymentDefaults.CAPTURED)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.PaidDateUtc = DateTime.UtcNow;
                    _orderService.UpdateOrder(order);
                }
            }
        }
    }
}
