using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.DBBL.Extensions;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Payments.DBBL.Controllers
{
    public class DBBLontroller : BasePaymentController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly DBBLPaymentSettings _dbblPaymentSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IPaymentPluginManager _paymentPluginManager;

        #endregion

        #region Ctor

        public DBBLontroller(ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            IPaymentPluginManager paymentPluginManager,
            IWorkContext workContext,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            DBBLPaymentSettings dbblPaymentSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _paymentPluginManager = paymentPluginManager;
            _workContext = workContext;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _dbblPaymentSettings = dbblPaymentSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> GatewayReturn()
        {
            var plugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(DBBLDefaults.SystemName);
            if (!plugin.PluginDescriptor.Installed || !_paymentPluginManager.IsPluginActive(plugin))
            {
                await _logger.ErrorAsync("DBBL Standard module cannot be loaded");
            }

            var transactionId = Request.Form["trans_id"];

            if (!string.IsNullOrEmpty(transactionId))
            {
                var orderId = await _genericAttributeService.GetAttributeAsync<int>(await _workContext.GetCurrentCustomerAsync(),
                    string.Format(DBBLDefaults.TransactionOrder, transactionId));

                if (await _orderService.GetOrderByIdAsync(orderId) is Order order)
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress);
                    var country = await _countryService.GetCountryByAddressAsync(billingAddress);

                    var billingInfo = "FIRSTNAME:" + billingAddress.FirstName +
                        "^LASTNAME:" + billingAddress.LastName +
                        "^STREET:" + billingAddress.Address1 +
                        "^CITY:" + billingAddress.City +
                        "^STATE:" + stateProvince?.Name +
                        "^POSTALCODE:" + billingAddress.ZipPostalCode +
                        "^COUNTRY:" + country?.TwoLetterIsoCode +
                        "^EMAIL:" + billingAddress.Email +
                        "^PHONENUMBER:" + billingAddress.PhoneNumber +
                        "^VIPCUSTOMER:no";

                    string resultResponse;
                    if (_dbblPaymentSettings.UseSandbox)
                    {
                        var client = new DBBL_WebService_Test.dbblecomtxnClient();
                        var resultReq = await client.getresultfieldAsync(
                            _dbblPaymentSettings.UserId, 
                            _dbblPaymentSettings.Password, 
                            transactionId, 
                            _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), 
                            billingInfo);

                        resultResponse = resultReq.Body.@return;
                    }
                    else
                    {
                        var client = new DBBL_WebService_Live.dbblecomtxnClient();
                        var resultReq = client.getresultfieldAsync(
                            _dbblPaymentSettings.UserId,
                            _dbblPaymentSettings.Password,
                            transactionId,
                            _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                            billingInfo);

                        resultResponse = resultReq.Result.Body.@return;
                    }

                    if (!string.IsNullOrEmpty(resultResponse))
                    {
                        var result = SubstringExtensions.Between(resultResponse, "RESULT>", "^RESULT_CODE>");
                        var resultCode = SubstringExtensions.Between(resultResponse, "^RESULT_CODE>", "^3DSECURE>");
                        var secure = SubstringExtensions.Between(resultResponse, "^3DSECURE>", "^RRN>");
                        var rrn = SubstringExtensions.Between(resultResponse, "^RRN>", "^APPROVAL_CODE>");
                        var approvalCode = SubstringExtensions.Between(resultResponse, "^APPROVAL_CODE>", "^CARD_NUMBER>");
                        var curdNumber = SubstringExtensions.Between(resultResponse, "^CARD_NUMBER>", "^AMOUNT>");
                        var amount = SubstringExtensions.Between(resultResponse, "^AMOUNT>", "^CARDNAME>");
                        var cardName = SubstringExtensions.Between(resultResponse, "^CARDNAME>", "^DESCRIPTION>");
                        var description = SubstringExtensions.Between(resultResponse, "^DESCRIPTION>", "^TRANS_DATE>");
                        var transactionDate = SubstringExtensions.After(resultResponse, "^TRANS_DATE>");

                        var sb = new StringBuilder();
                        sb.AppendLine("DBBL PDT:");
                        sb.AppendLine("Transaction Id: " + transactionId);
                        sb.AppendLine("RESULT: " + result);
                        sb.AppendLine("RESULT_CODE: " + resultCode);
                        sb.AppendLine("3DSECURE: " + secure);
                        sb.AppendLine("RRN: " + rrn);
                        sb.AppendLine("APPROVAL_CODE: " + approvalCode);
                        sb.AppendLine("CARD_NUMBER: " + curdNumber);
                        sb.AppendLine("AMOUNT: " + amount);
                        sb.AppendLine("CARDNAME: " + cardName);
                        sb.AppendLine("DESCRIPTION: " + description);
                        sb.AppendLine("TRANS_DATE: " + transactionDate);

                        //order note
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            Note = sb.ToString(),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = orderId
                        });

                        await _orderService.UpdateOrderAsync(order);

                        if (result == "OK" && resultCode == "000")
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        }
                    }
                }
                await _logger.ErrorAsync("DBBL PDT failed as order not found to mark as paid.");
                return RedirectToRoute("Homepage");
            }
            else
            {
                await _logger.ErrorAsync("DBBL PDT failed as transactionId null on request form.");
                return RedirectToRoute("Homepage");
            }
        }

        #endregion
    }
}
