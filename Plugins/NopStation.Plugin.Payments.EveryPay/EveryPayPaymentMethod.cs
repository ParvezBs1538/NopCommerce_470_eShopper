using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Services.Orders;
using Nop.Services.Cms;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Payments.EveryPay.Components;

namespace NopStation.Plugin.Payments.EveryPay
{
    public class EveryPayPaymentMethod : BasePlugin, IPaymentMethod, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly EveryPaySettings _everyPaySettings;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public EveryPayPaymentMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            EveryPaySettings everyPaySettings,
            IWorkContext workContext,
            ICurrencyService currencyService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _everyPaySettings = everyPaySettings;
            _workContext = workContext;
            _currencyService = currencyService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var token = "";
            var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(processPaymentRequest.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
            var paymentAmount = Convert.ToInt32(orderTotal * 100);

            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = true
            };

            switch ((TransactMode)_everyPaySettings.TransactModeId)
            {
                case TransactMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    result.AddError("Not supported transaction type");
                    break;
            }

            foreach (var item in processPaymentRequest.CustomValues)
            {
                if (item.Key == "ctn")
                    token = item.Value.ToString();
            }

            var payment = EveryPayAPI("payment", "payments", _everyPaySettings.PrivateKey, "POST", token.ToString(), paymentAmount, processPaymentRequest.OrderGuid.ToString());
            if (payment != null && payment["token"] != null)
            {
                result.AuthorizationTransactionId = payment["token"].ToString();
                result.CaptureTransactionResult = payment["status"] is null ? "" : payment["status"].ToString();
            }
            else if (payment["error"] != null)
            {
                result.AddError(payment["error"]["message"] is null ? "Failed" : payment["error"]["message"].ToString());
            }
            else
            {
                result.AddError("Failed");
            }

            return result;
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _everyPaySettings.AdditionalFee, _everyPaySettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var refundAmount = (int)(refundPaymentRequest.AmountToRefund * 100);
            var result = new RefundPaymentResult();
            var refund = EveryPayAPI("refund", "refunds", _everyPaySettings.PrivateKey, "POST", refundPaymentRequest.Order.AuthorizationTransactionId.ToString(), refundAmount, refundPaymentRequest.Order.OrderGuid.ToString());
            if (refund != null && refund["token"] != null)
            {
                result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
            }
            else if (refund["error"] != null)
            {
                result.AddError(refund["error"]["message"] is null ? "Failed" : refund["error"]["message"].ToString());
            }
            else
            {
                result.AddError("Failed");
            }

            return Task.FromResult(result);
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = true
            };
            switch ((TransactMode)_everyPaySettings.TransactModeId)
            {
                case TransactMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    result.AddError("Not supported transaction type");
                    break;
            }

            return Task.FromResult(result);
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(false);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            JObject cardInfo = null;
            var requestForm = new ProcessPaymentRequest();
            var endPoint = "tokens/";

            if (form.ContainsKey("Uuid"))
                requestForm.CustomValues.Add("uid", form["Uuid"].ToString());

            if (form.ContainsKey("Token"))
            {
                endPoint += form["Token"].ToString();
                requestForm.CustomValues.Add("ctn", form["Token"].ToString());
                cardInfo = EveryPayAPI("token", endPoint, _everyPaySettings.ApiKey, "GET");
            }

            if (cardInfo != null && cardInfo["card"] != null)
            {
                requestForm.CreditCardNumber = cardInfo["card"]["last_four"] is null ? "" : cardInfo["card"]["last_four"].ToString();
                requestForm.CreditCardName = cardInfo["card"]["friendly_name"] is null ? "" : cardInfo["card"]["friendly_name"].ToString();
                requestForm.CreditCardType = cardInfo["card"]["type"] is null ? "" : cardInfo["card"]["type"].ToString();
                requestForm.CreditCardExpireYear = cardInfo["card"]["expiration_year"] is null ? 0 : Convert.ToInt32(cardInfo["card"]["expiration_year"]);
                requestForm.CreditCardExpireMonth = cardInfo["card"]["expiration_month"] is null ? 0 : Convert.ToInt32(cardInfo["card"]["expiration_month"]);
            }

            return await Task.FromResult(requestForm);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/EveryPay/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(EveryPayViewComponent);
        }

        public override async Task InstallAsync()
        {
            var settings = new EveryPaySettings
            {
                TransactModeId = (int)TransactMode.AuthorizeAndCapture
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new EveryPayPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new EveryPayPermissionProvider());
            await base.UninstallAsync();
        }

        public JObject EveryPayAPI(string type, string apiEndPoint, string autKey, string httpType, string token = "", decimal amount = 0, string description = "")
        {
            var url = _everyPaySettings.UseSandbox ? "https://sandbox-api.everypay.gr" : "https://api.everypay.gr";
            url += "/" + apiEndPoint;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(httpType), url))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(autKey));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    switch (type)
                    {
                        case "payment":
                            var contentList = new List<string>();
                            contentList.Add("token=" + token);
                            contentList.Add("amount=" + amount);
                            contentList.Add("description=Payment for Order #" + description);
                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                            break;
                        case "refund":
                            contentList = new List<string>();
                            contentList.Add("payment=" + token);
                            contentList.Add("amount=" + amount);
                            contentList.Add("description=Refund for Order #" + description);
                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                            break;
                        case "token":
                            break;
                        default:
                            break;

                    }

                    var response = httpClient.SendAsync(request).Result;
                    var obj = response.Content.ReadAsStringAsync().Result;
                    var jsonValue = JObject.Parse(obj);
                    return jsonValue;
                }
            }
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Instructions", "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant."),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.TransactMode", "After checkout mark payment as"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.TransactMode.Hint", "Specify transaction mode."),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.ApiKey", "Public Key"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.ApiKey.Hint", "Please provide valid public key"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.PrivateKey", "Private Key"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.PrivateKey.Hint", "Please provide valid private key"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.Installments", "Installments"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.Installments.Hint", "Provide multiple installments decimal value with coma separated min value=2, max value=36"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.UseSandbox", "Use Sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration.Fields.UseSandbox.Hint", "Determine whether to use the sandbox environment for testing purposes."),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Configuration", "EveryPay settings"),

                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Menu.EveryPay", "EveryPay"),
                new KeyValuePair<string, string>("Admin.NopStation.EveryPay.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("NopStation.EveryPay.PaymentMethodDescription", "Pay by EveryPay credit / debit card"),
                new KeyValuePair<string, string>("NopStation.EveryPay.Button.Pay", "Pay")
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.EveryPay.PaymentMethodDescription");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.EveryPay.Menu.EveryPay"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(EveryPayPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.EveryPay.Menu.Configuration"),
                    Url = "~/Admin/EveryPay/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "EveryPay.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/every-pay-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=every-pay",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }
                
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer});
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(EveryPayPaymentScriptsViewComponent);
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        #endregion
    }
}