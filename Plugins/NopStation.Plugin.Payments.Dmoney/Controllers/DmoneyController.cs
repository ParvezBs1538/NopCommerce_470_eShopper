using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Dmoney.Models;
using NopStation.Plugin.Payments.Dmoney.Services;

namespace NopStation.Plugin.Payments.Dmoney.Controllers
{
    public class DmoneyController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly IDmoneyTransactionService _dmoneyTransactionService;
        private readonly DmoneyPaymentSettings _dmoneyPaymentSettings;
        private readonly IDmoneyPaymentService _dmoneyPaymentService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IProductService _productservice;

        public DmoneyController(IOrderService orderService,
            IDmoneyTransactionService dmoneyTransactionService,
            DmoneyPaymentSettings dmoneyPaymentSettings,
            IDmoneyPaymentService dmoneyPaymentService,
            INotificationService notificationService,
            IWorkContext workContext,
            IWebHelper webHelper,
            IProductService productservice)
        {
            _orderService = orderService;
            _dmoneyTransactionService = dmoneyTransactionService;
            _dmoneyPaymentSettings = dmoneyPaymentSettings;
            _dmoneyPaymentService = dmoneyPaymentService;
            _notificationService = notificationService;
            _workContext = workContext;
            _webHelper = webHelper;
            _productservice = productservice;
        }

        protected async Task<string> GetOrderDescriptionAsync(ICollection<OrderItem> orderItems)
        {
            var sb = new StringBuilder();
            foreach (var item in orderItems)
            {
                sb.AppendLine((await _productservice.GetProductByIdAsync(item.ProductId))?.Name);
            }
            return sb.ToString();
        }

        public async Task<IActionResult> Pay(string transactionTrackingNo)
        {
            var transaction = await _dmoneyTransactionService.GetTransactionByTrackingNumberAsync(transactionTrackingNo);
            if (transaction == null)
                return RedirectToRoute("HomePage");

            var order = await _orderService.GetOrderByIdAsync(transaction.OrderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId || order.PaymentMethodSystemName != "NopStation.Plugin.Payments.Dmoney")
                return RedirectToRoute("Homepage");

            var approveUrl = _webHelper.GetStoreLocation() + "dmoney/approve/" + WebUtility.UrlEncode(transactionTrackingNo);
            var cancelUrl = _webHelper.GetStoreLocation() + "dmoney/cancel/" + WebUtility.UrlEncode(transactionTrackingNo);
            var declineUrl = _webHelper.GetStoreLocation() + "dmoney/decline/" + WebUtility.UrlEncode(transactionTrackingNo);

            var model = new PaymentPublicModel()
            {
                ApproveUrl = approveUrl,
                SecretKey = _dmoneyPaymentSettings.SecretKey,
                BillerCode = _dmoneyPaymentSettings.BillerCode,
                CancelUrl = cancelUrl,
                DeclineUrl = declineUrl,
                GatewayUrl = _dmoneyPaymentSettings.GatewayUrl,
                Description = await GetOrderDescriptionAsync(await _orderService.GetOrderItemsAsync(order.Id)),
                OrderId = order.Id,
                OrderTotal = order.OrderTotal,
                OrganizationCode = _dmoneyPaymentSettings.OrganizationCode,
                Password = _dmoneyPaymentSettings.Password,
                TransactionTrackingNo = transactionTrackingNo
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Dmoney/Views/Pay.cshtml", model);
        }

        public async Task<IActionResult> Approve(string transactionTrackingNo)
        {
            var result = await _dmoneyPaymentService.VerifyTransactionAsync(transactionTrackingNo);

            if (result.Status)
                return RedirectToRoute("OrderDetails", new { orderId = result.OrderId });

            if (result.Errors.Any())
                _notificationService.ErrorNotification(result.Errors.FirstOrDefault());

            return RedirectToRoute("HomePage");
        }

        public IActionResult Cancel(string transactionTrackingNo)
        {
            return RedirectToRoute("HomePage");
        }

        public IActionResult Decline(string transactionTrackingNo)
        {
            return RedirectToRoute("HomePage");
        }
    }
}
