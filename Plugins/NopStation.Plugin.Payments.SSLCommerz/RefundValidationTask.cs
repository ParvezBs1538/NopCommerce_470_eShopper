using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Payments;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Payments.SSLCommerz.Sevices;

namespace NopStation.Plugin.Payments.SSLCommerz
{
    public class RefundValidationTask : IScheduleTask
    {
        private readonly IRefundService _refundService;
        private readonly ISSLCommerzManager _commerzManager;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;

        public RefundValidationTask(IRefundService refundService,
            ISSLCommerzManager commerzManager,
            ILogger logger,
            IOrderService orderService)
        {
            _refundService = refundService;
            _commerzManager = commerzManager;
            _logger = logger;
            _orderService = orderService;
        }

        public async Task ExecuteAsync()
        {
            var pageIndex = 0;

            while (true)
            {
                var refunds = await _refundService.SearchRefundsAsync(
                    refunded: false,
                    maxCheckCount: 4,
                    pageIndex: pageIndex,
                    pageSize: 100);

                if (!refunds.Any())
                    break;

                foreach (var refund in refunds)
                {
                    try
                    {
                        var order = await _orderService.GetOrderByIdAsync(refund.OrderId);
                        var result = await _commerzManager.VerifyRefundAsync(refund, order);
                        if (result.Success)
                        {
                            refund.Refunded = true;
                            refund.InitiatedOn = result.InitiatedOn;
                            refund.RefundedOn = result.RefundedOn;

                            await _refundService.UpdateRefundAsync(refund);

                            order.PaymentStatus = PaymentStatus.Refunded;
                            await _orderService.UpdateOrderAsync(order);
                        }
                        else
                        {
                            refund.StatusCheckCount++;
                            await _logger.ErrorAsync($"Refund failed. {Environment.NewLine}" +
                                $"Order Id: {refund.OrderId}, {Environment.NewLine}Refrence Id: {refund.RefrenceId}");

                            await _refundService.UpdateRefundAsync(refund);
                        }
                    }
                    catch (Exception ex)
                    {
                        refund.StatusCheckCount++;
                        await _refundService.UpdateRefundAsync(refund);

                        await _logger.ErrorAsync(ex.Message, ex);
                    }
                }

                pageIndex++;
            }
        }
    }
}