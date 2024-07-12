using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Payments;
using NopStation.Plugin.Payments.BlueSnapHosted.Services;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Controllers
{
    public class BlueSnapWebhookController : BaseController
    {
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IBlueSnapServices _blueSnapService;

        public BlueSnapWebhookController(IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IBlueSnapServices blueSnapService)
        {
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _blueSnapService = blueSnapService;
        }

        [HttpPost]
        [HttpsRequirement]
        public async Task<IActionResult> WebhookHandler()
        {
            try
            {
                if (Request.Form.TryGetValue("transactionType", out var transactionType) && transactionType.Equals("RECURRING"))
                {
                    var initialOrder = await _orderService.GetOrderByGuidAsync(new Guid(Request.Form["productName"]));
                    if (initialOrder == null)
                        return Ok();

                    //try to get related recurring payment
                    var recurringPayment = (await _orderService.SearchRecurringPaymentsAsync(initialOrderId: initialOrder.Id)).FirstOrDefault();
                    if (recurringPayment == null)
                        return Ok();

                    var transactionId = await _blueSnapService.BlueSnapSubscriptionTransactionIdAsync(Request.Form["subscriptionId"]);

                    if (string.IsNullOrWhiteSpace(transactionId))
                        return Ok();

                    var recurringPaymentsHistory = (await _orderService.GetRecurringPaymentHistoryAsync(recurringPayment))
                        .OrderBy(historyEntry => historyEntry.CreatedOnUtc).ToList();

                    var chargesForSub = await _blueSnapService.BlueSnapChargesForSubscriptionIdAsync(initialOrder.SubscriptionTransactionId);

                    var chargedHistory = 0;
                    var pendingOrders = 0;
                    var pendingOrderIds = new List<int>();

                    foreach (var history in recurringPaymentsHistory)
                    {
                        var order = await _orderService.GetOrderByIdAsync(history.OrderId);
                        if (order.PaymentStatus == PaymentStatus.Pending)
                        {
                            pendingOrders++;
                            pendingOrderIds.Add(order.Id);
                        }

                        if (order.PaymentStatus == PaymentStatus.Paid)
                            chargedHistory++;
                    }

                    var leftForProcess = chargesForSub.Charges.Count - chargedHistory;

                    if (pendingOrders > 0)
                    {
                        for (var i = 0; i < pendingOrders; i++)
                        {
                            if (leftForProcess > 0)
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
            catch { }

            return Ok();
        }
    }
}
