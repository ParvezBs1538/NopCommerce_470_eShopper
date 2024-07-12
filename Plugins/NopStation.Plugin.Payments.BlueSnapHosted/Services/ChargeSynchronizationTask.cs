using System;
using System.Linq;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using System.Collections.Generic;
using Nop.Services.Payments;
using Nop.Core.Domain.Payments;
using System.Threading.Tasks;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Services
{
    public class ChargeSynchronizationTask : IScheduleTask
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IBlueSnapServices _blueSnapServices;

        #endregion

        #region Ctor

        public ChargeSynchronizationTask(IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            IBlueSnapServices blueSnapServices)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _blueSnapServices = blueSnapServices;
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

                if (remainingCycles > 0 && await _orderProcessingService.GetNextPaymentDateAsync(recurringPayment) is DateTime nextPaymentDate)
                {
                    //get recurring payments history
                    var recurringPaymentsHistory = (await _orderService.GetRecurringPaymentHistoryAsync(recurringPayment))
                        .OrderBy(historyEntry => historyEntry.CreatedOnUtc).ToList();

                    // fetch all the charges from bluesnap and check if payment is made for pending subscription histories 
                    var initialOrder = await _orderService.GetOrderByIdAsync(recurringPayment.InitialOrderId);

                    var chargesForSub = await _blueSnapServices.BlueSnapChargesForSubscriptionIdAsync(initialOrder.SubscriptionTransactionId);

                    var chargedHistory = 0;
                    var pendingOrders = 0;
                    var pendingOrderIds = new List<int>();
                    
                    recurringPaymentsHistory.ForEach(async history =>
                    {
                        var order = await _orderService.GetOrderByIdAsync(history.OrderId);
                        if (order.PaymentStatus == PaymentStatus.Pending)
                        {
                            pendingOrders++;
                            pendingOrderIds.Add(order.Id);
                        }

                        if (order.PaymentStatus == PaymentStatus.Paid)
                        {
                            chargedHistory++;
                        }
                    });

                    var leftForProcess = chargesForSub.Charges.Count - chargedHistory; 

                    if (pendingOrders > 0)
                    {
                        for (var i = 0; i < pendingOrders; i++)
                        {
                            if(leftForProcess > 0 )
                            {
                                var pendingOrder = pendingOrderIds[i];
                                var order = await _orderService.GetOrderByIdAsync(pendingOrder);
                                if (order.PaymentStatus != PaymentStatus.Paid)
                                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                                leftForProcess--;
                            }
                        }

                        while (leftForProcess > 0)
                        {
                            await _orderProcessingService.ProcessNextRecurringPaymentAsync(recurringPayment, new ProcessPaymentResult
                            {
                                SubscriptionTransactionId = initialOrder.SubscriptionTransactionId,
                                NewPaymentStatus = PaymentStatus.Paid
                            });
                            leftForProcess--;
                        }
                    }
                    else
                    {
                        await _orderProcessingService.ProcessNextRecurringPaymentAsync(recurringPayment, new ProcessPaymentResult
                        {
                            SubscriptionTransactionId = initialOrder.SubscriptionTransactionId,
                            NewPaymentStatus = PaymentStatus.Paid
                        });
                    }
                }
            }
        }

        #endregion
    }
}
