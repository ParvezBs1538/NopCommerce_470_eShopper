using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Services.Orders;
using NopStation.Plugin.Payments.iPayBd.Models.Request;
using NopStation.Plugin.Payments.iPayBd.Models.Response;
using RestSharp;
using Newtonsoft.Json;
using NopStation.Plugin.Payments.iPayBd.Components;

namespace NopStation.Plugin.Payments.iPayBd
{
    public class IpayBdPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IpayBdPaymentSettings _ipayBdPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public IpayBdPaymentProcessor(IpayBdPaymentSettings ipayBdPaymentSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IHttpContextAccessor httpContextAccessor,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderService orderService)
        {
            _ipayBdPaymentSettings = ipayBdPaymentSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;

            var callbackBaseUrl = $"{_webHelper.GetStoreLocation()}ipaybd/";
            var requestModel = new CreateOrderRequestModel()
            { 
                Amount = order.OrderTotal,
                ReferenceId = $"{Guid.NewGuid}",
                SuccessCallbackUrl = $"{callbackBaseUrl}success/{order.OrderGuid}",
                CancelCallbackUrl = $"{callbackBaseUrl}cancelled/{order.OrderGuid}",
                FailureCallbackUrl = $"{callbackBaseUrl}failed/{order.OrderGuid}",
            };

            var createOrderUrl = IpayDefaults.GetBaseUrl(_ipayBdPaymentSettings.Sandbox).Concat("order");
            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {_ipayBdPaymentSettings.ApiKey}");

            var client = new RestClient(createOrderUrl);
            request.AddParameter("application/json", JsonConvert.SerializeObject(requestModel), ParameterType.RequestBody);

            var response = client.Execute(request);

            var responseModel = JsonConvert.DeserializeObject<CreateOrderResponseModel>(response.Content);
            if (response.IsSuccessful && responseModel.Message == "Order placed successfully")
            { 
                order.AuthorizationTransactionCode = responseModel.ReferenceId;
                order.AuthorizationTransactionId = responseModel.OrderId;
                await _orderService.UpdateOrderAsync(order);
                _httpContextAccessor.HttpContext.Response.Redirect(responseModel.PaymentUrl);
            }
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _ipayBdPaymentSettings.AdditionalFee, _ipayBdPaymentSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(true);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var request = new ProcessPaymentRequest();
            return Task.FromResult(request);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentIpayBd/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(IpayBdViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new IpayBdPaymentSettings
            {
                DescriptionText = "<p>You will be redirected to iPay payment gateway.</p>",
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new IpayBdPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<IpayBdPaymentSettings>();
            await this.UninstallPluginAsync(new IpayBdPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.iPayBd.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Menu.iPayBd", "iPay"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Menu.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration", "iPay settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.AdditionalFee.Hint", "The additional fee."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.DescriptionText", "Description"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.ApiKey", "Api key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.ApiKey.Hint", "Enter iPay api key."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.Sandbox", "Sandbox"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.iPayBd.Configuration.Fields.Sandbox.Hint", "Check to enable testing environment (sandbox)."));

            list.Add(new KeyValuePair<string, string>("NopStation.iPayBd.PaymentMethodDescription", "Pay by iPay"));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.iPayBd.Menu.iPayBd"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(IpayBdPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.iPayBd.Menu.Configuration"),
                    Url = "~/Admin/PaymentIpayBd/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "iPayBd.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/ipay-bd-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=ipay-bd-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }
                
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        #endregion
    }
}