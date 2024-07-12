using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Payments.CopyAndPay.Models;
using NopStation.Plugin.Payments.CopyAndPay.Services;

namespace NopStation.Plugin.Payments.CopyAndPay.Controllers
{
    public class PaymentCopyAndPayController : BasePluginController
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly ICopyAndPayServices _cOPYandPayServices;
        private readonly IWebHelper _webHelper;
        private readonly CopyAndPayPaymentSettings _copyAndPayPaymentSettings;
        private readonly IOrderService _orderService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public PaymentCopyAndPayController(ILogger logger,
            ICopyAndPayServices cOPYandPayServices,
            IWebHelper webHelper,
            CopyAndPayPaymentSettings copyAndPayPaymentSettings,
            IOrderService orderService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _logger = logger;
            _cOPYandPayServices = cOPYandPayServices;
            _webHelper = webHelper;
            _copyAndPayPaymentSettings = copyAndPayPaymentSettings;
            _orderService = orderService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Payment(string orderId, string selectedBrand)
        {
            var dataBrands = string.Empty;
            var result = int.TryParse(orderId, out int idOrder);

            if (result == false)
                return RedirectToRoute("CopyAndPayPayment", new { orderId });

            string[] brands = { "VISA", "MASTER", "AMEX" };
            if (!string.IsNullOrEmpty(_copyAndPayPaymentSettings.MadaEntityId))
            {
                Array.Resize(ref brands, brands.Length + 1);
                brands[^1] = "MADA";
            }
            if (string.IsNullOrEmpty(selectedBrand))
            {
                dataBrands = string.Join(" ", brands);
                selectedBrand = brands.First();
            }
            else
            {
                dataBrands = selectedBrand + " " + string.Join(" ", brands.Where(x => x != selectedBrand));
            }

            var hyperRequestForm = await _cOPYandPayServices.RequestFormAsync(idOrder, selectedBrand);

            if (hyperRequestForm == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.CopyAndPay.Payment.Initialization.Failed"));
            }

            var id = hyperRequestForm == null ? "" : hyperRequestForm["id"];
            var validateUrl = $"{_webHelper.GetStoreLocation()}copyandpay/validatepayment/{orderId}";
            var paymentUrl = $"{_webHelper.GetStoreLocation()}copyandpay/payment/{orderId}";

            var model = new CopyAndPayModel
            {
                FormId = id,
                ValidateUrl = validateUrl,
                RequestUrl = _copyAndPayPaymentSettings.APIUrl,
                PaymentUrl = paymentUrl,
                DataBrands = dataBrands,
                SelectedBrand = selectedBrand
            };

            return View("~/Plugins/NopStation.Plugin.Payments.CopyAndPay/Views/Payment.cshtml", model);
        }

        public async Task<IActionResult> ValidatePayment(string orderId, string id, string resourcePath)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(resourcePath))
            {
                await _logger.ErrorAsync("PaymentCopyAndPayController => ValidatePament => id or resourcePath is null");
                return RedirectToRoute("CopyAndPayPayment", new { orderId });
            }

            var resultOrder = int.TryParse(orderId, out var idOrdere);
            if (resultOrder == false)
                return RedirectToRoute("CopyAndPayPayment", new { orderId });

            if (!_cOPYandPayServices.RequestPaymentStatus(idOrdere, id, out var responseData))
                return RedirectToRoute("CopyAndPayPayment", new { orderId });

            var result = responseData.Result;
            var paidStatusRegex = new Regex(@"^(000\.000\.|000\.100\.1|000\.[36])");

            if (paidStatusRegex.Match(result.Code).Success)
            {

                var order = await _orderService.GetOrderByIdAsync(idOrdere);
                order.PaymentStatus = PaymentStatus.Paid;
                order.OrderStatus = OrderStatus.Processing;
                order.AuthorizationTransactionId = responseData.Id;
                order.AuthorizationTransactionResult = result.Description;
                order.CardNumber = responseData.Card.Last4Digits;
                order.CardName = responseData.Card.Holder;
                order.CardExpirationYear = responseData.Card.ExpiryYear;
                order.CardExpirationMonth = responseData.Card.ExpiryMonth;

                await _orderService.UpdateOrderAsync(order);

                return RedirectToRoute("CheckoutCompleted", new { orderId });
            }
            else
            {
                _notificationService.ErrorNotification(result.Description);
                await _logger.WarningAsync(result.Description);
            }

            return RedirectToRoute("CopyAndPayPayment", new { orderId });
        }

        #endregion
    }
}
