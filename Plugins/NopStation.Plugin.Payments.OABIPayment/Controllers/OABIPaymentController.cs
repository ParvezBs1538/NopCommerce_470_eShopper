using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.OABIPayment.Models;

namespace NopStation.Plugin.Payments.OABIPayment.Controllers
{
    public class OABIPaymentController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly OABIPaymentSettings _oabIPaymentSettings;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public OABIPaymentController(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            OABIPaymentSettings paykeeperPaymentSettings,
            ILogger logger)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _oabIPaymentSettings = paykeeperPaymentSettings;
            _logger = logger;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Success()
        {
            try
            {
                var response = GetData();
                var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(response.OrderId));

                if (order is { Deleted: false } && (response.Status == "CAPTURED"))
                {
                    if (order.CustomerId.ToString() == response.CustomerId)
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = response.PaymentId;
                            order.CaptureTransactionResult = "Payment successful";
                            await _orderService.UpdateOrderAsync(order);
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        }
                        else
                        {
                            var note = $"Payment successful, But Could not mark as Paid. Transaction Id: {response.PaymentId}";
                            await _orderService.InsertOrderNoteAsync(new OrderNote
                            {
                                OrderId = order.Id,
                                Note = note,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                }
                else
                {
                    await _logger.InformationAsync($"OAB Payment failed for orderId: {response.OrderId}, paymentId:{response.PaymentId}, status: {response.Status}");
                }

                return RedirectToAction("Details", "Order", new { orderId = response.OrderId });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Fail()
        {
            try
            {
                var response = GetData();
                await _logger.InformationAsync($"OAB Payment faild for orderId: {response.OrderId}, paymentId:{response.PaymentId}, status: {response.Status}");
                return RedirectToAction("Details", "Order", new { orderId = response.OrderId });
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion

        #region Utility

        private ResponseData GetData()
        {
            var response = new Dictionary<string, string>();

            if (Request.Form.Count != 0 && Request.Form.ContainsKey("trandata"))
            {
                var strTrandata = Request.Form["trandata"].ToString();
                var strKey = _oabIPaymentSettings.ResourceKey;
                var key = Encoding.ASCII.GetBytes(strKey);
                var numberOfChar = strTrandata.Length / 2;
                var data = new byte[numberOfChar];
                var sr = new StringReader(strTrandata);
                for (var i = 0; i < numberOfChar; i++)
                    data[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
                sr.Dispose();
                var tdes = TripleDES.Create();
                tdes.Key = key;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.Zeros;
                var ict = tdes.CreateDecryptor();
                var enc = ict.TransformFinalBlock(data, 0, data.Length);
                var decryptedStr = Encoding.ASCII.GetString(enc);

                if (decryptedStr.StartsWith("<"))
                {
                    decryptedStr = decryptedStr.Substring(0, decryptedStr.LastIndexOf('>') + 1);
                }
                else
                {
                    decryptedStr = decryptedStr.Substring(0, decryptedStr.LastIndexOf('&') + 1);
                }

                decryptedStr = decryptedStr.Trim();
                string[] lines = System.Text.RegularExpressions.Regex.Split(decryptedStr, "&");
                foreach (string line in lines)
                {
                    if (line == null || line.Trim().Length == 0)
                        continue;
                    string[] lines1 = System.Text.RegularExpressions.Regex.Split(line, "=");
                    response.Add(lines1[0], lines1[1]);
                }
            }

            return new ResponseData
            {
                Amount = response.ContainsKey("amt") ? response["amt"] : string.Empty,
                Status = response.ContainsKey("result") ? response["result"] : string.Empty,
                TrackId = response.ContainsKey("trackid") ? response["trackid"] : string.Empty,
                OrderId = response.ContainsKey("udf1") ? response["udf1"] : string.Empty,
                CustomerId = response.ContainsKey("udf2") ? response["udf2"] : string.Empty,
                PaymentId = response.ContainsKey("paymentid") ? response["paymentid"] : string.Empty
            };
        }

        #endregion
    }
}
