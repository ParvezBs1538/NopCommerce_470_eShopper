using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using System;
using Nop.Services.Orders;
using RestSharp;
using Newtonsoft.Json;
using NopStation.Plugin.Payments.iPayBd.Models.Response;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Payments.iPayBd.Controllers
{
    public class IpayBdController : NopStationAdminController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IpayBdPaymentSettings _ipayBdPaymentSettings;
        private readonly IOrderProcessingService _orderProcessingService;

        #endregion

        #region Ctor

        public IpayBdController(IOrderService orderService,
            IpayBdPaymentSettings ipayBdPaymentSettings,
            IOrderProcessingService orderProcessingService)
        {
            _orderService = orderService;
            _ipayBdPaymentSettings = ipayBdPaymentSettings;
            _orderProcessingService = orderProcessingService;
        }

        #endregion

        #region Methods

        public IActionResult Success(Guid orderGuid)
        {
            return RedirectToRoute("iPayBdProcess", new { orderGuid = orderGuid, status = "SUCCESS" });
        }

        public IActionResult Failed(Guid orderGuid)
        {
            return RedirectToRoute("iPayBdProcess", new { orderGuid = orderGuid, status = "FAILED" });
        }

        public IActionResult Cancelled(Guid orderGuid)
        {
            return RedirectToRoute("iPayBdProcess", new { orderGuid = orderGuid, status = "CANCELLED" });
        }

        public async Task<IActionResult> Process(Guid orderGuid, string status)
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order.Deleted)
                return RedirectToRoute("Homepage");

            if (status == "SUCCESS")
            {
                var checkStatusUrl = IpayDefaults.GetBaseUrl(_ipayBdPaymentSettings.Sandbox).Concat("order", order.AuthorizationTransactionId, "status");

                var client = new RestClient(checkStatusUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Bearer {_ipayBdPaymentSettings.ApiKey}");

                var response = client.Execute(request);
                var responseModel = JsonConvert.DeserializeObject<OrderStatusResponseModel>(response.Content);

                if (responseModel.StatusCode == 200)
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);

                        var orderNote = new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            DisplayToCustomer = false,
                            Note = $"Order paid by iPay: {Environment.NewLine}{JsonConvert.SerializeObject(responseModel, Formatting.Indented)}",
                            OrderId = order.Id
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);
                    }
                }
                else
                {
                    var orderNote = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        Note = $"Failed to pay order ({status}): {Environment.NewLine}{JsonConvert.SerializeObject(responseModel, Formatting.Indented)}",
                        OrderId = order.Id
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);
                }
            }

            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        #endregion
    }
}