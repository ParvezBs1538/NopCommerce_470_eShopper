using System;
using System.Linq;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using System.Collections.Generic;
using Nop.Services.Payments;
using Nop.Core.Domain.Payments;
using System.Threading.Tasks;
using NopStation.Plugin.Payments.NetsEasy.Models.Response;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nop.Services.Logging;

namespace NopStation.Plugin.Payments.NetsEasy.Services
{
    public class ChargeSynchronizationTask : IScheduleTask
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ChargeSynchronizationTask(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
        }

        #endregion

        #region Task

        public async Task ExecuteAsync()
        {
            //get recurringPayments

            var recurringPayments = await _orderService.SearchRecurringPaymentsAsync(showHidden: true);

            foreach (var recurringPayment in recurringPayments)
            {
                if (!recurringPayment.IsActive)
                    continue;

                var remainingCycles = await _orderProcessingService.GetCyclesRemainingAsync(recurringPayment);

                if (remainingCycles > 0 && await _orderProcessingService.GetNextPaymentDateAsync(recurringPayment) is DateTime nextPaymentDate 
                    && DateTime.UtcNow.Subtract(nextPaymentDate).TotalSeconds>=0)
                {
                    var initialOrder = await _orderService.GetOrderByIdAsync(recurringPayment.InitialOrderId);
                    if(initialOrder != null && string.Equals(initialOrder.PaymentMethodSystemName, NetsEasyPaymentDefaults.PluginSystemName,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        var errors = (await _orderProcessingService.ProcessNextRecurringPaymentAsync(recurringPayment)).ToList();
                        if (errors.Any())
                        {
                            await _logger.ErrorAsync($"Recurring Schedule Task Error For intial order #Order {recurringPayment.InitialOrderId}", new Exception(string.Join('\n', errors)));
                        }
                    }
                }
            }
        }

        #endregion
    }
}
