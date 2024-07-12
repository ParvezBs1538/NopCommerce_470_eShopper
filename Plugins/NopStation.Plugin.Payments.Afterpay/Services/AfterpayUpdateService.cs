using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Services.Orders;

namespace NopStation.Plugin.Payments.Afterpay.Services
{
    public class AfterpayUpdateService : IAfterpayUpdateService
    {
        private readonly IOrderService _orderService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IAfterpayPaymentService _afterpayRequestService;

        public AfterpayUpdateService(IOrderService orderService,
            IRepository<Order> orderRepository,
            IAfterpayPaymentService afterpayRequestService)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _afterpayRequestService = afterpayRequestService;
        }

        public async Task UpdateOrderPaymentStatusAsync()
        {
            var orders = await _orderRepository.Table.Where(x =>
                            x.PaymentMethodSystemName == AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME &&
                            x.PaymentStatusId == (int)PaymentStatus.Pending &&
                            x.CreatedOnUtc.AddMinutes(30) >= DateTime.UtcNow)
                        .ToListAsync();

            foreach (var order in orders)
            {
                var capturedResponse = await _afterpayRequestService.GetCapturedResponseAsync(order);
                if (capturedResponse.Status == AfterpayPaymentDefaults.APPROVED && capturedResponse.PaymentState == AfterpayPaymentDefaults.CAPTURED)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.PaidDateUtc = DateTime.UtcNow;
                    await _orderService.UpdateOrderAsync(order);
                }
            }
        }
    }
}
