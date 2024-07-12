using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/order")]
    public class OrderApiController : BaseApiController
    {
        #region Fields

        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IShipmentService _shipmentService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public OrderApiController(IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IShipmentService shipmentService,
            IWebHelper webHelper,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            ICustomerService customerService)
        {
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _shipmentService = shipmentService;
            _webHelper = webHelper;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        //My account / Orders
        [HttpGet("history")]
        public virtual async Task<IActionResult> CustomerOrders()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
            return OkWrap(model);
        }

        //My account / Orders / Cancel recurring order
        [HttpPost("cancelrecurringpayment")]
        public virtual async Task<IActionResult> CancelRecurringPayment([FromBody] List<KeyValueApi> formValues)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            var form = formValues == null ? new NameValueCollection() : formValues.ToNameValueCollection();
            //get recurring payment identifier
            var recurringPaymentId = 0;
            foreach (string formValue in form.Keys)
                if (formValue.StartsWith("cancelRecurringPayment", StringComparison.InvariantCultureIgnoreCase))
                    recurringPaymentId = Convert.ToInt32(formValue.Substring("cancelRecurringPayment".Length));

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(recurringPaymentId);
            if (recurringPayment == null)
                return NotFound();

            if (await _orderProcessingService.CanCancelRecurringPaymentAsync(await _workContext.GetCurrentCustomerAsync(), recurringPayment))
            {
                var errors = await _orderProcessingService.CancelRecurringPaymentAsync(recurringPayment);

                var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync();
                model.RecurringPaymentErrors = errors;
                return OkWrap(model, errors: errors);
            }

            return BadRequest();
        }

        //My account / Reward points
        [HttpGet("customerrewardpoints/{pageNumber?}")]
        public virtual async Task<IActionResult> CustomerRewardPoints(int? pageNumber)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Unauthorized();

            if (!_rewardPointsSettings.Enabled)
                return BadRequest();

            var model = await _orderModelFactory.PrepareCustomerRewardPointsAsync(pageNumber);
            return OkWrap(model);
        }

        //My account / Order details page
        [HttpGet("orderdetails/{orderId:min(0)}")]
        public virtual async Task<IActionResult> Details(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return NotFound();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
            return OkWrap(model);
        }

        //My account / Order details page / Print
        [HttpGet("orderdetails/print/{orderId}")]
        public virtual async Task<IActionResult> PrintOrderDetails(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return NotFound();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
            model.PrintMode = true;

            return OkWrap(model);
        }

        //My account / Order details page / PDF invoice
        [HttpGet("orderdetails/pdf/{orderId}")]
        public virtual async Task<IActionResult> GetPdfInvoice(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return NotFound();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _pdfService.PrintOrderToPdfAsync(stream, order, await _workContext.GetWorkingLanguageAsync());
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }

        //My account / Order details page / re-order
        [HttpGet("reorder/{orderId:min(0)}")]
        public virtual async Task<IActionResult> ReOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return NotFound();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            await _orderProcessingService.ReOrderAsync(order);
            return Ok();
        }

        //My account / Order details page / Complete payment

        [HttpPost("orderdetails/repostpayment/{orderId:min(0)}")]
        public virtual async Task<IActionResult> RePostPayment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return NotFound();

            if ((await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            if (!await _paymentService.CanRePostProcessPaymentAsync(order))
                return BadRequest();

            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };
            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Ok();
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return Redirect($"api/orderdetails/{orderId}");
        }

        //My account / Order details page / Shipment details page
        [HttpGet("orderdetails/shipment/{shipmentId}")]
        public virtual async Task<IActionResult> ShipmentDetails(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return Unauthorized();

            var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);

            return OkWrap(model);
        }

        #endregion
    }
}
