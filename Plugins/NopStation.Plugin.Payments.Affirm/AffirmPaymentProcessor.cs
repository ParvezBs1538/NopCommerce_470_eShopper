using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Affirm.Components;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Models;
using NopStation.Plugin.Payments.Affirm.Services;

namespace NopStation.Plugin.Payments.Affirm
{
    public class AffirmPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly AffirmPaymentSettings _affirmPaymentSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAffirmPaymentTransactionService _affirmPaymentTransactionService;
        private readonly WidgetSettings _widgetSettings;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly string _sandboxVoidURL = "https://sandbox.affirm.com/api/v2/charges/{0}/void";
        private readonly string _voidURL = "https://api.affirm.com/api/v2/charges/{0}/void";
        private readonly string _canadaSandboxVoidURL = "https://sandbox.affirm.ca/api/v2/charges/{0}/void";
        private readonly string _canadaVoidURL = "https://api.affirm.ca/api/v2/charges/{0}/void";
        private readonly string _sandboxCaptureURL = "https://sandbox.affirm.com/api/v2/charges/{0}/capture";
        private readonly string _captureURL = "https://api.affirm.com/api/v2/charges/{0}/capture";
        private readonly string _canadaSandboxCaptureURL = "https://sandbox.affirm.ca/api/v2/charges/{0}/capture";
        private readonly string _canadaCaptureURL = "https://api.affirm.ca/api/v2/charges/{0}/capture";
        private readonly string _sandboxRefundURL = "https://sandbox.affirm.com/api/v2/charges/{0}/refund";
        private readonly string _refundURL = "https://api.affirm.com/api/v2/charges/{0}/refund";
        private readonly string _canadaSandboxRefundURL = "https://sandbox.affirm.ca/api/v2/charges/{0}/refund";
        private readonly string _canadaRefundURL = "https://api.affirm.ca/api/v2/charges/{0}/refund";

        #endregion

        #region Ctor
        public AffirmPaymentProcessor(ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            AffirmPaymentSettings affirmPaymentSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IAffirmPaymentTransactionService affirmPaymentTransactionService,
            WidgetSettings widgetSettings,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _affirmPaymentSettings = affirmPaymentSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _affirmPaymentTransactionService = affirmPaymentTransactionService;
            _widgetSettings = widgetSettings;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => true;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => true;

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            var processPaymentResult = new ProcessPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Pending
            };

            return Task.FromResult(processPaymentResult);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var orderReference = await _genericAttributeService.GetAttributeAsync<Guid>(await _workContext.GetCurrentCustomerAsync(), AffirmPaymentDefaults.OrderId, (await _storeContext.GetCurrentStoreAsync()).Id);
            var transaction = await _affirmPaymentTransactionService.GetTransactionByReferenceAsync(orderReference);

            if (transaction != null)
            {
                if (_affirmPaymentSettings.TransactionMode == TransactionMode.Authorize)
                {
                    await _orderProcessingService.MarkAsAuthorizedAsync(postProcessPaymentRequest.Order);

                    await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), AffirmPaymentDefaults.OrderId, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                    transaction.OrderGuid = postProcessPaymentRequest.Order.OrderGuid;
                    await _affirmPaymentTransactionService.UpdateTransactionAsync(transaction);
                }
                else
                {
                    var cHARGE_ID = transaction.ChargeId;
                    string captureURL;
                    if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                        captureURL = string.Format(_affirmPaymentSettings.UseSandbox ? _sandboxCaptureURL : _captureURL, cHARGE_ID);
                    else
                        captureURL = string.Format(_affirmPaymentSettings.UseSandbox ? _canadaSandboxCaptureURL : _canadaCaptureURL, cHARGE_ID);

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), captureURL))
                        {
                            var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_affirmPaymentSettings.PublicApiKey}:{_affirmPaymentSettings.PrivateApiKey}"));
                            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                            var response = await httpClient.SendAsync(request);

                            if (response.IsSuccessStatusCode)
                            {
                                var responseContent = response.Content;
                                CaptureJsonModel responseModel = null;

                                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                                {
                                    var text = reader.ReadToEndAsync();
                                    responseModel = JsonConvert.DeserializeObject<CaptureJsonModel>(await text);
                                }

                                if (responseModel.Amount >= decimal.ToInt32(postProcessPaymentRequest.Order.OrderTotal * 100))
                                {
                                    postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Paid;
                                    postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Processing;
                                    await _orderService.UpdateOrderAsync(postProcessPaymentRequest.Order);
                                }

                                await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), AffirmPaymentDefaults.OrderId, null, (await _storeContext.GetCurrentStoreAsync()).Id);
                                transaction.OrderGuid = postProcessPaymentRequest.Order.OrderGuid;
                                await _affirmPaymentTransactionService.UpdateTransactionAsync(transaction);
                            }
                        }
                    }
                }
            }
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var currentCustomerCurrencyId = (await _workContext.GetCurrentCustomerAsync()).CurrencyId;
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currentCustomerCurrencyId ?? 0);
            //customer currency currently supports only USD and CAD
            //https://docs.affirm.com/affirm-developers/reference#data-types-4

            //var currencyTmp = await _currencyService.GetCurrencyByIdAsync(
            //    await _genericAttributeService.GetAttributeAsync<int>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CurrencyIdAttribute, (await _storeContext.GetCurrentStoreAsync()).Id));
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

            if (customerCurrency.CurrencyCode == "USD" || customerCurrency.CurrencyCode == "CAD")
                return false;
            return true;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
               _affirmPaymentSettings.AdditionalFee, _affirmPaymentSettings.AdditionalFeePercentage);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            if (capturePaymentRequest == null)
                throw new ArgumentException(nameof(capturePaymentRequest));

            var result = new CapturePaymentResult();
            var order = capturePaymentRequest.Order;

            var transaction = await _affirmPaymentTransactionService.GetTransactionByReferenceAsync(order.OrderGuid);
            if (transaction != null)
            {
                var cHARGE_ID = transaction.ChargeId;
                string captureURL;
                if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                    captureURL = string.Format(_affirmPaymentSettings.UseSandbox ? _sandboxCaptureURL : _captureURL, cHARGE_ID);
                else
                    captureURL = string.Format(_affirmPaymentSettings.UseSandbox ? _canadaSandboxCaptureURL : _canadaCaptureURL, cHARGE_ID);

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), captureURL))
                    {
                        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_affirmPaymentSettings.PublicApiKey}:{_affirmPaymentSettings.PrivateApiKey}"));
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                        var response = await httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = response.Content;
                            CaptureJsonModel responseModel = null;

                            using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
                            {
                                var text = reader.ReadToEndAsync().Result;
                                responseModel = JsonConvert.DeserializeObject<CaptureJsonModel>(text);
                            }

                            result.NewPaymentStatus = PaymentStatus.Paid;
                            result.CaptureTransactionId = responseModel.TransactionId;
                        }
                        else
                            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.AffirmPayment.TransactionNotFound"));
                    }
                }
            }
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            if (refundPaymentRequest == null)
                throw new ArgumentException(nameof(refundPaymentRequest));

            var result = new RefundPaymentResult();
            var order = refundPaymentRequest.Order;

            var transaction = await _affirmPaymentTransactionService.GetTransactionByReferenceAsync(order.OrderGuid);
            if (transaction != null)
            {
                var cHARGE_ID = transaction.ChargeId;
                string refundURL;
                if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                    refundURL = string.Format(_affirmPaymentSettings.UseSandbox ? _sandboxRefundURL : _refundURL, cHARGE_ID);
                else
                    refundURL = string.Format(_affirmPaymentSettings.UseSandbox ? _canadaSandboxRefundURL : _canadaRefundURL, cHARGE_ID);

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), refundURL))
                    {
                        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_affirmPaymentSettings.PublicApiKey}:{_affirmPaymentSettings.PrivateApiKey}"));
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                        request.Content = new StringContent("{\"amount\": " + decimal.ToInt32(refundPaymentRequest.AmountToRefund * 100) + "}");
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ?
                                PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
                        }
                        else
                            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.AffirmPayment.TransactionNotFound"));
                    }
                }
            }

            return result;
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            if (voidPaymentRequest == null)
                throw new ArgumentException(nameof(voidPaymentRequest));

            var result = new VoidPaymentResult();
            var order = voidPaymentRequest.Order;

            var transaction = await _affirmPaymentTransactionService.GetTransactionByReferenceAsync(order.OrderGuid);
            if (transaction != null)
            {
                var cHARGE_ID = transaction.ChargeId;
                var voidURL = string.Empty;
                if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                    voidURL = string.Format(_affirmPaymentSettings.UseSandbox ? _sandboxVoidURL : _voidURL, cHARGE_ID);
                else
                    voidURL = string.Format(_affirmPaymentSettings.UseSandbox ? _canadaSandboxVoidURL : _canadaVoidURL, cHARGE_ID);

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), voidURL))
                    {
                        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_affirmPaymentSettings.PublicApiKey}:{_affirmPaymentSettings.PrivateApiKey}"));
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                        var response = await httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            result.NewPaymentStatus = PaymentStatus.Voided;
                        }
                        else
                            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.AffirmPayment.TransactionNotFound"));
                    }
                }
            }

            return result;
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            throw new NotImplementedException();
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest == null)
                throw new ArgumentException(nameof(cancelPaymentRequest));

            //always success
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AffirmPayment/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(AffirmPaymentViewComponent);
        }
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new AffirmPaymentSettings
            {
                TransactionMode = TransactionMode.AuthorizeAndCapture,
                UseSandbox = true,
                EnableOnProductBox = false,
                CountryAPIMode = CountryAPIMode.USA
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(AffirmPaymentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(AffirmPaymentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.InstallPluginAsync(new AffirmPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(AffirmPaymentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(AffirmPaymentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.DeleteSettingAsync<AffirmPaymentSettings>();

            await this.UninstallPluginAsync(new AffirmPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            if (form.TryGetValue("Errors", out var errorsString) && !StringValues.IsNullOrEmpty(errorsString))
            {
                warnings.AddRange(errorsString.ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            return Task.FromResult<IList<string>>(warnings);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return Task.FromResult(paymentInfo);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.PublicApiKey", "Public api key"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.PublicApiKey.Hint", "Enter public api key."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.PrivateApiKey", "Private api key"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.PrivateApiKey.Hint", "Enter private api key."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.FinancialProductKey", "Financial product key"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.FinancialProductKey.Hint", "Enter financial product key."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.MerchantName", "Merchant name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.MerchantName.Hint", "Optional. If you have multiple sites operating under a single Affirm account, you can override the external company/brand name that the customer sees. This affects all references to your company name in the Affirm UI."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.UseSandbox.Hint", "Determine whether to use sandbox credentials."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.EnableOnProductBox", "Enable on product box"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.EnableOnProductBox.Hint", "Enable affirm promotional message on product box."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.TransactionMode", "Transaction mode"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.TransactionMode.Hint", "Choose the transaction mode."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.CountryAPIMode", "Country API"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.CountryAPIMode.Hint", "Country API mode."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Configuration.Fields.PaymentMethodDescription", "Checkout by affirm payment"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Menu.AffirmPayment", "Affirm payment"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AffirmPayment.Configuration", "Affirm payment settings")
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffirmPayment.Menu.AffirmPayment"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AffirmPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffirmPayment.Menu.Configuration"),
                    Url = "~/Admin/AffirmPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AffirmPayment.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/affirm-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=affirm-payment",
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
            var list = new List<string>
            {
                PublicWidgetZones.OpCheckoutPaymentMethodBottom,
                PublicWidgetZones.OrderSummaryContentDeals,
                PublicWidgetZones.ProductDetailsOverviewBottom
            };

            if (_affirmPaymentSettings.EnableOnProductBox)
                list.Add(PublicWidgetZones.ProductBoxAddinfoBefore);

            return Task.FromResult<IList<string>>(list);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.ProductBoxAddinfoBefore || widgetZone == PublicWidgetZones.ProductDetailsOverviewBottom
                || widgetZone == PublicWidgetZones.OrderSummaryContentDeals)
                return typeof(AffirmPromotionalMessageViewComponent);

            if (widgetZone == PublicWidgetZones.OpCheckoutPaymentMethodBottom)
                return typeof(AffirmPaymentMethodMessageViewComponent);
            return null;

        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Admin.NopStation.AffirmPayment.Configuration.Fields.PaymentMethodDescription");
        }

        #endregion
    }
}