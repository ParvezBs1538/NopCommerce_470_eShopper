using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Dmoney.Domains;
using NopStation.Plugin.Payments.Dmoney.Services;

namespace NopStation.Plugin.Payments.Dmoney
{
    public class DmoneyPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IDmoneyTransactionService _dmoneyTransactionService;
        private readonly DmoneyPaymentSettings _dmoneyPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public DmoneyPaymentProcessor(IWebHelper webHelper,
            IDmoneyTransactionService dmoneyTransactionService,
            DmoneyPaymentSettings dmoneyPaymentSettings,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _webHelper = webHelper;
            _dmoneyTransactionService = dmoneyTransactionService;
            _dmoneyPaymentSettings = dmoneyPaymentSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _settingService = settingService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Properies

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => true;

        public string PaymentMethodDescription => _dmoneyPaymentSettings.Description;

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DmoneyPayment/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new DmoneyPaymentSettings()
            {
                GatewayUrl = "https://api.dmoney.com.bd:8181/external-payment-web/payment/create",
                TransactionVerificationUrl = "https://api.dmoney.com.bd:8181/external-payment-web/payment/getStatus"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new DmoneyPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new DmoneyPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.RecurringNotSupported"));
            return result;
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.PaymentStatus != PaymentStatus.Pending)
                return Task.FromResult(false);

            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.CaptureNotSupported"));
            return result;
        }

        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult<decimal>(0);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var trackingNo = $"{Guid.NewGuid()}__{order.OrderGuid}";

            var transaction = new DmoneyTransaction()
            {
                Amount = order.OrderTotal,
                CreatedOnUtc = DateTime.UtcNow,
                LastUpdatedOnUtc = DateTime.UtcNow,
                TransactionStatus = TransactionStatus.Init,
                StatusCode = 0,
                OrderId = order.Id,
                TransactionTrackingNumber = trackingNo
            };

            await _dmoneyTransactionService.InsertTransactionAsync(transaction);

            var url = $"{_webHelper.GetStoreLocation()}checkout/dmoney/{trackingNo}";
            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.RecurringNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.RefundNotSupported"));
            return result;
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.VoidNotSupported"));
            return result;
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            throw new NotImplementedException();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.DmoneyPayment.Menu.DmoneyPayment"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(DmoneyPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DmoneyPayment.Menu.Configuration"),
                    Url = "~/Admin/DmoneyPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DmoneyPayment.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/dmoney-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=dmoney-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration", "Dmoney payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Menu.DmoneyPayment", "Dmoney payment"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.RecurringNotSupported", "Recurring method not supported"),

                new KeyValuePair<string, string>("NopStation.DmoneyPayment.Transaction.TransactionNotFound", "Transaction not found."),
                new KeyValuePair<string, string>("NopStation.DmoneyPayment.Transaction.OrderNotFound", "Order not found."),
                new KeyValuePair<string, string>("NopStation.DmoneyPayment.Transaction.Invalid", "Invalid transaction."),

                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.GatewayUrl", "Gateway url"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.TransactionVerificationUrl", "Transaction verification url"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.OrganizationCode", "Organization code"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.Password", "Password"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.BillerCode", "Biller code"),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.Description", "Description"),

                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.GatewayUrl.Hint", "The 'Gateway url' for Dmoney payment."),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.TransactionVerificationUrl.Hint", "The 'Transaction verification url' for Dmoney payment."),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.OrganizationCode.Hint", "The 'Organization code' for Dmoney payment."),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.Password.Hint", "The 'Password' for Dmoney payment."),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.SecretKey.Hint", "The 'Secret key' for Dmoney payment."),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.BillerCode.Hint", "The 'Biller code' for Dmoney payment."),
                new KeyValuePair<string, string>("Admin.NopStation.DmoneyPayment.Configuration.Fields.Description.Hint", "The 'Description' for Dmoney payment.")
            };

            return list;
        }

        public Task<string> GetPaymentMethodDescriptionAsync()
        {
            return Task.FromResult("DMoney Payment Method");
        }

        #endregion
    }
}
