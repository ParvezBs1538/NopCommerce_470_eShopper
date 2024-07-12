using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using Nop.Services.Security;
using NopStation.Plugin.Payments.DBBL.Components;

namespace NopStation.Plugin.Payments.DBBL
{
    public class DBBLPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IWebHelper _webHelper;
        private readonly DBBLPaymentSettings _dbblPaymentSettings;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public DBBLPaymentProcessor(IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            IWebHelper webHelper,
            DBBLPaymentSettings dbblPaymentSettings,
            ILogger logger,
            IWorkContext workContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderService orderService)
        {
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _webHelper = webHelper;
            _dbblPaymentSettings = dbblPaymentSettings;
            _logger = logger;
            _workContext = workContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderService = orderService;
        }

        #endregion

        #region Utilities

        private string GetTransactionIdReponse(PostProcessPaymentRequest postProcessPaymentRequest, string selectedCardTypeValue)
        {
            string transactionIdResponse;
            if (_dbblPaymentSettings.UseSandbox)
            {
                var client = new DBBL_WebService_Test.dbblecomtxnClient();
                var request = client.getransidAsync(_dbblPaymentSettings.UserId,
                _dbblPaymentSettings.Password,
                Math.Truncate(postProcessPaymentRequest.Order.OrderTotal * 100).ToString(),
                selectedCardTypeValue,
                postProcessPaymentRequest.Order.Id.ToString(),
                _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
                transactionIdResponse = request.Result.Body.@return;
            }
            else
            {
                var client = new DBBL_WebService_Live.dbblecomtxnClient();
                var request = client.getransidAsync(_dbblPaymentSettings.UserId,
                _dbblPaymentSettings.Password,
                Math.Truncate(postProcessPaymentRequest.Order.OrderTotal * 100).ToString(),
                selectedCardTypeValue,
                postProcessPaymentRequest.Order.Id.ToString(),
                _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
                transactionIdResponse = request.Result.Body.@return;
            }

            return transactionIdResponse;
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
            var customValues = _paymentService.DeserializeCustomValues(order);

            if (!customValues.ContainsKey(DBBLDefaults.CardType))
                throw new NopException("Selcted card type could not be found.");

            var cardType = customValues[DBBLDefaults.CardType].ToString();

            try
            {
                var transactionIdResponse = GetTransactionIdReponse(postProcessPaymentRequest, cardType);

                if (transactionIdResponse.StartsWith("TRANSACTION_ID:"))
                {
                    var transactionId = transactionIdResponse.Remove(transactionIdResponse.IndexOf("TRANSACTION_ID:"), "TRANSACTION_ID:".Length);

                    if (transactionId.Length == 28)
                    {
                        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                            string.Format(DBBLDefaults.TransactionOrder, transactionId), order.Id);

                        order.AuthorizationTransactionId = transactionId;
                        await _orderService.UpdateOrderAsync(order);

                        transactionId = HttpUtility.UrlEncode(transactionId);
                        string gateWayUrl;
                        if (_dbblPaymentSettings.UseSandbox)
                            gateWayUrl = string.Format(DBBLDefaults.TestGateWayUrl, cardType, transactionId);
                        else
                            gateWayUrl = string.Format(DBBLDefaults.LiveGateWayUrl, cardType, transactionId);

                        _httpContextAccessor.HttpContext.Response.Redirect(gateWayUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("DBBL get tran id fail for order id" + postProcessPaymentRequest.Order.Id + ". exception: " + ex.Message, ex);
            }
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _dbblPaymentSettings.AdditionalFee, _dbblPaymentSettings.AdditionalFeePercentage);
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

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            if (!form.ContainsKey("dbbl_card") || string.IsNullOrWhiteSpace(form["dbbl_card"]))
                return new List<string>() { await _localizationService.GetResourceAsync("NopStation.DBBL.Checkout.DBBLCard.Required") };

            return new List<string>();
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var request = new ProcessPaymentRequest();
            if (form.ContainsKey("dbbl_card") && !string.IsNullOrWhiteSpace(form["dbbl_card"]))
                request.CustomValues[DBBLDefaults.CardType] = form["dbbl_card"].ToString();

            return Task.FromResult(request);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentDBBL/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(DBBLViewComponent);
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new DBBLPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new DBBLPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>(new Dictionary<string, string>
            {
                ["Admin.NopStation.DBBL.Configuration.Fields.AdditionalFee"] = "Additional fee",
                ["Admin.NopStation.DBBL.Configuration.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Admin.NopStation.DBBL.Configuration.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Admin.NopStation.DBBL.Configuration.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Admin.NopStation.DBBL.Configuration.Fields.UseSandbox"] = "Use Sandbox",
                ["Admin.NopStation.DBBL.Configuration.Fields.UseSandbox.Hint"] = "Check to enable Sandbox (testing environment).",
                ["Admin.NopStation.DBBL.Configuration.Fields.UserId"] = "User Id",
                ["Admin.NopStation.DBBL.Configuration.Fields.UserId.Hint"] = "Specify your DBBL user Id.",
                ["Admin.NopStation.DBBL.Configuration.Fields.Password"] = "Password",
                ["Admin.NopStation.DBBL.Configuration.Fields.Password.Hint"] = "Specify your DBBL password.",
                ["Admin.NopStation.DBBL.Configuration"] = "DBBL settings",
                ["Admin.NopStation.DBBL.Configuration"] = "DBBL settings",
                ["Admin.NopStation.DBBL.Menu.DBBL"] = "DBBL",
                ["Admin.NopStation.DBBL.Menu.Configuration"] = "Configuration",
                ["NopStation.DBBL.Checkout.DBBLCard.Required"] = "DBBL card must be selected.",
                ["NopStation.DBBL.PaymentMethodDescription"] = "Pay with VISA, MASTERCARD, Nexus, Rocket",
            });

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.DBBL.Menu.DBBL"),
                Visible = true,
                IconClass = "far fa-dot-circle",

            };

            if (await _permissionService.AuthorizeAsync(DBBLPaymentPermissionProvider.ManageConfiguration))
            {
                var config = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DBBL.Menu.Configuration"),
                    Url = "~/Admin/PaymentDBBL/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DBBL.Configuration"
                };
                menuItem.ChildNodes.Add(config);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/dbbl-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=dbbl-payment",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.DBBL.PaymentMethodDescription");
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        #endregion
    }
}