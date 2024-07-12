using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Payments.AamarPay.Controllers;

public class AamarPayPaymentController : NopStationPublicController
{
    private readonly ILogger _logger;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;

    public AamarPayPaymentController(ILogger logger,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService)
    {
        _logger = logger;
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
    }

    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Success()
    {
        try
        {
            var response = string.Empty;
            var dict = new Dictionary<string, string>();
            foreach (var key in HttpContext.Request.Form.Keys)
            {
                var value = HttpContext.Request.Form[key];
                dict.Add(key, value);
            }

            response = JsonConvert.SerializeObject(dict);
            var order = await _orderService.GetOrderByGuidAsync(new Guid(dict["mer_txnid"]));

            if (dict["pay_status"] == "Successful" && order != null)
            {
                order.CaptureTransactionId = dict["mer_txnid"];
                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                await _orderService.UpdateOrderAsync(order);

                await WriteOrderNote(dict["mer_txnid"], "AamarPay pyment successful");

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            await WriteOrderNote(dict["mer_txnid"], "AamarPay payment is unsuccessful");
        }
        catch (Exception ex)
        {
            _logger.Error("AamarPay payment failed ", ex);
        }

        return RedirectToRoute("Homepage");
    }

    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Failure()
    {
        try
        {
            var response = string.Empty;
            var dict = new Dictionary<string, string>();
            foreach (var key in HttpContext.Request.Form.Keys)
            {
                var value = HttpContext.Request.Form[key];
                dict.Add(key, value);
            }

            response = JsonConvert.SerializeObject(dict);
            await WriteOrderNote(dict["mer_txnid"], "AamarPay payment failed.");

        }
        catch (Exception ex)
        {
            _logger.Error("AamarPay payment failed ", ex);
        }

        return RedirectToRoute("Homepage");
    }

    [HttpPost]
    public IActionResult Cancelled()
    {
        return RedirectToRoute("Homepage");
    }

    private async Task WriteOrderNote(string orderRef, string message)
    {
        var order = await _orderService.GetOrderByGuidAsync(new Guid(orderRef));
        if (order == null)
            return;

        await _orderService.InsertOrderNoteAsync(new OrderNote()
        {
            OrderId = order.Id,
            CreatedOnUtc = DateTime.UtcNow,
            Note = message,
            DisplayToCustomer = false
        });
    }
}