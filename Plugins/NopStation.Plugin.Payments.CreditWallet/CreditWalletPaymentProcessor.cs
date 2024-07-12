using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
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
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Components;
using NopStation.Plugin.Payments.CreditWallet.Components;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet
{
    public class CreditWalletPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IWalletService _walletService;
        private readonly IActivityHistoryService _activityHistoryService;
        private readonly IWorkContext _workContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly CreditWalletSettings _creditWalletSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ICurrencyService _currencyService;

        public CreditWalletPaymentProcessor(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IActivityHistoryService activityHistoryService,
            IWalletService walletService,
            IWorkContext workContext,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            CreditWalletSettings creditWalletSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService,
            WidgetSettings widgetSettings,
            ICurrencyService currencyService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _activityHistoryService = activityHistoryService;
            _walletService = walletService;
            _workContext = workContext;
            _priceFormatter = priceFormatter;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _creditWalletSettings = creditWalletSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _customerService = customerService;
            _widgetSettings = widgetSettings;
            _currencyService = currencyService;
        }

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => _creditWalletSettings.SkipPaymentInfo;

        public bool HideInWidgetList => true;

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return Task.FromResult(false);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capturing not supported" } });
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _creditWalletSettings.AdditionalFee, _creditWalletSettings.AdditionalFeePercentage);
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return await Task.FromResult(new ProcessPaymentRequest());
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            if (_creditWalletSettings.ShowAvailableCreditOnCheckoutPage)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var wallet = await _walletService.GetWalletByCustomerIdAsync(customer.Id);
                var balance = await _priceFormatter.FormatPriceAsync((wallet?.Active ?? false) ? wallet.AvailableCredit : 0, true, wallet == null ? await _workContext.GetWorkingCurrencyAsync() : await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId));
                return string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Balance"), balance);
            }
            return await _localizationService.GetResourceAsync("NopStation.CreditWallet.PaymentMethodDescription");
        }

        public Type GetPublicViewComponent()
        {
            return typeof(CreditWalletViewComponent);
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            if (!_creditWalletSettings.HideMethodIfInsufficientBalance)
                return false;

            if (cart.Count > 0)
            {
                var wallet = await _walletService.GetWalletByCustomerIdAsync(cart.First().CustomerId);
                if (wallet != null && wallet.Active)
                {
                    var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, usePaymentMethodAdditionalFee: false);
                    var orderTotal = shoppingCartTotal ?? 0 + await GetAdditionalHandlingFeeAsync(cart);

                    return !await _walletService.HasSufficientBalance(wallet, orderTotal);
                }
            }

            return true;
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var wallet = await _walletService.GetWalletByCustomerIdAsync(order.CustomerId);

            if (wallet != null && wallet.Active && (wallet.AllowOverspend || await _walletService.HasSufficientBalance(wallet, order.OrderTotal)))
            {
                await _orderProcessingService.MarkOrderAsPaidAsync(order);

                var orderNote = new OrderNote()
                {
                    OrderId = order.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.PaidByCreditWallet")
                };
                await _orderService.InsertOrderNoteAsync(orderNote);

                var walletCurrency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);
                var orderCreditTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, walletCurrency);

                wallet.AvailableCredit -= orderCreditTotal;
                wallet.CreditUsed += orderCreditTotal;
                await _walletService.UpdateWalletAsync(wallet);

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                var prevActivity = (await _activityHistoryService.GetWalletActivityHistoryAsync(wallet)).FirstOrDefault();
                var activity = new ActivityHistory()
                {
                    ActivityType = ActivityType.OrderPlaced,
                    CreatedByCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    WalletCustomerId = wallet.WalletCustomerId,
                    Note = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.OrderPlacedActivity"),
                        orderCreditTotal, order.Id, customer.Email),
                    CurrentTotalCreditUsed = prevActivity?.CurrentTotalCreditUsed ?? 0 + orderCreditTotal,
                    PreviousTotalCreditUsed = prevActivity?.CurrentTotalCreditUsed ?? 0
                };
                await _activityHistoryService.InsertActivityHistoryAsync(activity);
            }
        }

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var wallet = await _walletService.GetWalletByCustomerIdAsync(processPaymentRequest.CustomerId);
            if (wallet == null || !wallet.Active)
            {
                var errors = new List<string> { await _localizationService.GetResourceAsync("NopStation.CreditWallet.InvalidWallet") };
                return new ProcessPaymentResult() { Errors = errors };
            }

            if (!wallet.AllowOverspend && !await _walletService.HasSufficientBalance(wallet, processPaymentRequest.OrderTotal))
            {
                var errors = new List<string> { await _localizationService.GetResourceAsync("NopStation.CreditWallet.InsufficientBalance") };
                return new ProcessPaymentResult() { Errors = errors };
            }

            return new ProcessPaymentResult();
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CreditWallet/Configure";
        }

        public override async Task InstallAsync()
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            var creditWalletSettings = new CreditWalletSettings
            {
                DescriptionText = "Wallet payment",
                SkipPaymentInfo = false
            };

            await _settingService.SaveSettingAsync(creditWalletSettings);
            await this.InstallPluginAsync(new CreditWalletPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (Version.Parse(currentVersion) < Version.Parse("4.50.1.3"))
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
                {
                    ["Admin.NopStation.CreditWallet.Configuration.Fields.HideMethodIfInsufficientBalance"] = "Hide if insufficient balance",
                    ["Admin.NopStation.CreditWallet.Configuration.Fields.HideMethodIfInsufficientBalance.Hint"] = "Hide payment method if balance is lower than cart amount.",
                    ["Admin.NopStation.CreditWallet.Wallets.SaveBeforeEdit"] = "You need to save the customer before you can update wallet.",
                });
            }
            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.DeleteSettingAsync<CreditWalletSettings>();
            await this.UninstallPluginAsync(new CreditWalletPermissionProvider());
            await base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.CustomerDetailsBlock,
                PublicWidgetZones.AccountNavigationAfter,
                PublicWidgetZones.OrderSummaryTotals
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
                return typeof(CreditWalletAdminViewComponent);

            if (widgetZone == PublicWidgetZones.OrderSummaryTotals)
                return typeof(CreditWalletCartBalanceViewComponent);

            return typeof(CreditWalletCustomerNavViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Menu.CreditWallet"),
                Visible = true,
                IconClass = "far fa-dot-circle",

            };

            if (await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
            {
                var config = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Menu.Configuration"),
                    Url = "~/Admin/CreditWallet/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CreditWallet.Configuration"
                };
                menuItem.ChildNodes.Add(config);

                var wallet = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Menu.Wallets"),
                    Url = "~/Admin/Wallet/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Wallets"
                };
                menuItem.ChildNodes.Add(wallet);

                var invoice = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Menu.InvoicePayment"),
                    Url = "~/Admin/WalletInvoicePayment/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CreditWallet.InvoicePayment"
                };
                menuItem.ChildNodes.Add(invoice);

                var activity = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.Menu.Activities"),
                    Url = "~/Admin/WalletActivityHistory/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CreditWallet.ActivityHistory"
                };
                menuItem.ChildNodes.Add(activity);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/credit-wallet-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=credit-wallet",
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
            var list = new List<KeyValuePair<string, string>>(new Dictionary<string, string>
            {
                ["Admin.NopStation.CreditWallet.Balance"] = "Your balance: {0}",
                ["Admin.NopStation.CreditWallet.Menu.CreditWallet"] = "Credit wallet",
                ["Admin.NopStation.CreditWallet.Menu.Wallets"] = "Wallets",
                ["Admin.NopStation.CreditWallet.Menu.Activities"] = "Activities",
                ["Admin.NopStation.CreditWallet.Menu.Configuration"] = "Configuration",
                ["Admin.NopStation.CreditWallet.Menu.InvoicePayment"] = "Invoices",

                ["Admin.NopStation.CreditWallet.Wallet.Created"] = "Wallet has been created successfully.",
                ["Admin.NopStation.CreditWallet.Wallet.Updated"] = "Wallet has been updated successfully.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Created"] = "Invoice payment has been created successfully.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Updated"] = "Invoice payment has been updated successfully.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Deleted"] = "Invoice payment has been deleted successfully.",

                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.WalletCustomer"] = "Wallet",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.ActivityType"] = "Activity type",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.PreviousTotalCreditUsed"] = "Prev. total credit used",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.CurrentTotalCreditUsed"] = "Current total credit used",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreatedByCustomer"] = "Created by",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreatedOn"] = "Created on",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.Note"] = "Note",
                ["Admin.NopStation.CreditWallet.ActivityHistory.Fields.CreditUsed"] = "Credit used",

                ["Admin.NopStation.CreditWallet.ActivityHistory.List"] = "Activities",
                ["Admin.NopStation.CreditWallet.ActivityHistory.List.SearchWalletCustomer"] = "Search by customer",
                ["Admin.NopStation.CreditWallet.ActivityHistory.List.SearchEmail"] = "Email",
                ["Admin.NopStation.CreditWallet.ActivityHistory.List.SearchEmail.Hint"] = "Search by customer email.",
                ["Admin.NopStation.CreditWallet.ActivityHistory.List.SelectedActivityTypes"] = "Activity types",
                ["Admin.NopStation.CreditWallet.ActivityHistory.List.SelectedActivityTypes.Hint"] = "Search by activity types.",

                ["Admin.NopStation.CreditWallet.Configuration.Fields.AdditionalFee"] = "Additional fee",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.SkipPaymentInfo"] = "Skip payment info",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.SkipPaymentInfo.Hint"] = "Check to skip payment info in checkout.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.DescriptionText"] = "Description text",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.DescriptionText.Hint"] = "Enter payment method description text. It will be displayed in checkout payment info step.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.HideMethodIfInsufficientBalance"] = "Hide if insufficient balance",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.HideMethodIfInsufficientBalance.Hint"] = "Hide payment method if balance is lower than cart amount.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.ShowAvailableCreditOnCheckoutPage"] = "Show balance in checkout page",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.ShowAvailableCreditOnCheckoutPage.Hint"] = "Show available credit on checkout page.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.ShowInvoicesInCustomerWalletPage"] = "Show invoices in customer wallet page",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.ShowInvoicesInCustomerWalletPage.Hint"] = "Check to show invoices in customer wallet page.",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.MaxInvoicesToShowInCustomerWalletPage"] = "Max invoices to show",
                ["Admin.NopStation.CreditWallet.Configuration.Fields.MaxInvoicesToShowInCustomerWalletPage.Hint"] = "Define max invoices to show in customer wallet page.",

                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.WalletCustomer"] = "Wallet",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.WalletCustomer.Hint"] = "The customer wallet.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.InvoiceReference"] = "Invoice reference",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.InvoiceReference.Hint"] = "Enter payment invoice reference.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.PaymentDate"] = "Payment date",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.PaymentDate.Hint"] = "Enter invoice payment date.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.Amount"] = "Amount",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.Amount.Hint"] = "Enter paid amount with this invoice.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.CreatedByCustomer"] = "Created by",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.CreatedByCustomer.Hint"] = "The user who created the invoice.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.UpdatedByCustomer"] = "Updated by",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.UpdatedByCustomer.Hint"] = "The user who updated the invoice.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.Note"] = "Note",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.Note.Hint"] = "Enter invoice note.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.AddNew"] = "Add new invoice",
                ["Admin.NopStation.CreditWallet.InvoicePayments.EditDetails"] = "Edit invoice details",
                ["Admin.NopStation.CreditWallet.InvoicePayments.BackToCustomer"] = "back to customer details",

                ["Admin.NopStation.CreditWallet.InvoicePayments.List"] = "Invoices",
                ["Admin.NopStation.CreditWallet.InvoicePayments.List.SearchInvoiceReference"] = "Invoice reference",
                ["Admin.NopStation.CreditWallet.InvoicePayments.List.SearchInvoiceReference.Hint"] = "Search by invoice reference.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.List.SearchCreatedFrom"] = "Created from",
                ["Admin.NopStation.CreditWallet.InvoicePayments.List.SearchCreatedFrom.Hint"] = "Search by created from.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.List.SearchCreatedTo"] = "Created to",
                ["Admin.NopStation.CreditWallet.InvoicePayments.List.SearchCreatedTo.Hint"] = "Search by created to.",

                ["Admin.NopStation.CreditWallet.Wallets.SaveBeforeEdit"] = "You need to save the customer before you can update wallet.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.WalletCustomer"] = "Customer",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.WalletCustomer.Hint"] = "The wallet customer.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.Active"] = "Active",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.Active.Hint"] = "Check to mark the wallet as active.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.CreditLimit"] = "Credit limit",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.CreditLimit.Hint"] = "Define credit limit for this wallet.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.CreditUsed"] = "Credit used",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.CreditUsed.Hint"] = "Total credit used from this wallet.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.AvailableCredit"] = "Available credit",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.AvailableCredit.Hint"] = "Available credit for this wallet.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.AllowOverspend"] = "Allow overspend",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.AllowOverspend.Hint"] = "Check to allowover spend by this wallet.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.Currency"] = "Currency",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.Currency.Hint"] = "Select currency for this wallet.",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.WarnUserForCreditBelow"] = "Warn user for credit below",
                ["Admin.NopStation.CreditWallet.Wallets.Fields.WarnUserForCreditBelow.Hint"] = "When the available credit falls below (reaches) this quantity, the customer will see a warning in customer wallet page.",

                ["Admin.NopStation.CreditWallet.Wallets.List.SearchFirstName"] = "First name",
                ["Admin.NopStation.CreditWallet.Wallets.List.SearchFirstName.Hint"] = "Search by wallet customer first name.",
                ["Admin.NopStation.CreditWallet.Wallets.List.SearchLastName"] = "Last name",
                ["Admin.NopStation.CreditWallet.Wallets.List.SearchLastName.Hint"] = "Search by wallet customer last name.",
                ["Admin.NopStation.CreditWallet.Wallets.List.SearchEmail"] = "Email",
                ["Admin.NopStation.CreditWallet.Wallets.List.SearchEmail.Hint"] = "Search by wallet customer email.",
                ["Admin.NopStation.CreditWallet.Wallets.List.CustomerRoles"] = "Customer roles",
                ["Admin.NopStation.CreditWallet.Wallets.List.CustomerRoles.Hint"] = "Search by wallet customer roles.",

                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.InvoiceReference.Required"] = "Invoice reference is required.",
                ["Admin.NopStation.CreditWallet.InvoicePayments.Fields.PaymentDate.Required"] = "Payment date is required.",

                ["Admin.NopStation.CreditWallet.WalletDetails"] = "Wallet details",
                ["Admin.NopStation.CreditWallet.InvoicePayments"] = "Invoice payments",
                ["Admin.NopStation.CreditWallet.Activities"] = "Wallet activities",

                ["Admin.NopStation.CreditWallet.Configuration"] = "Credit wallet settings",
                ["Admin.NopStation.CreditWallet.Wallets.List"] = "Wallets",

                ["Admin.NopStation.CreditWallet.PaidByCreditWallet"] = "Order paid by Wallet",
                ["Admin.NopStation.CreditWallet.OrderPlacedActivity"] = "{0} credited from wallet for order (id# {1}) - by {2}",
                ["Admin.NopStation.CreditWallet.Wallet.UpdateActivity.NoDiff"] = "Wallet updated without change of credit limit - by {0}",
                ["Admin.NopStation.CreditWallet.Wallet.UpdateActivity.Increased"] = "Available credit increased by {0} due to change of credit limit - by {1}",
                ["Admin.NopStation.CreditWallet.Wallet.UpdateActivity.Decreased"] = "Available credit reduced by {0} due to change of credit limit - by {1}",
                ["Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity.NoDiff"] = "Wallet updated without change of credit limit - by {0}",
                ["Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity.Increased"] = "Available credit increased by {0} due to change of invoice (id#{1}) - by {2}",
                ["Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity.Decreased"] = "Available credit reduced by {0} due to change of invoice (id#{1}) - by {2}",
                ["Admin.NopStation.CreditWallet.Wallet.CreateActivity"] = "Wallet initiated. Initial credit limit {0} - by {1}",
                ["Admin.NopStation.CreditWallet.InvoicePayments.CreateActivity"] = "Available credit increased by {0} due to new invoice payment (id# {1}) added - by {2}",
                ["Admin.NopStation.CreditWallet.InvoicePayments.UpdateActivity"] = "Available credit reduced by {0} due to invoice payment (id# {1}) modified - by {2}",
                ["Admin.NopStation.CreditWallet.InvoicePayments.DeleteActivity"] = "Available credit reduced by {0} due to invoice payment (id# {1}) deleted - by {2}",
                ["Admin.NopStation.CreditWallet.OrderCancelledActivity"] = "Available credit increased by {0} due to order (id# {1}) cancelled added - by {2}",
                ["NopStation.CreditWallet.InsufficientBalance"] = "Order cannot be paid due to insufficient balance.",
                ["NopStation.CreditWallet.InvalidWallet"] = "Wallet is invalid or inactive.",
                ["NopStation.CreditWallet.PaymentMethodDescription"] = "Pay by Wallet",
                ["NopStation.CreditWallet.CustomerNav"] = "Wallet details",

                ["NopStation.CreditWallet.Account.WalletInfo"] = "Wallet Info",
                ["NopStation.CreditWallet.Wallet.CurrentBalance"] = "Your current balance is {0} credit ({1}). Total credit used {2} ({3}).",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.History"] = "Payment History",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.NoHistory"] = "No Invoice Payment.",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.Fields.PaymentDate"] = "Payment Date",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.Fields.InvoiceReference"] = "Reference",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.Fields.Amount"] = "Amount",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.Fields.Note"] = "Note",
                ["NopStation.CreditWallet.Wallet.InvoicePayment.Fields.PaymentDate"] = "PaymentDate",
                ["NopStation.CreditWallet.Account.LowCreditWarning"] = "You have low credit.",
                ["NopStation.CreditWallet.Account.InActiveWarning"] = "Your wallet is not active.",
            });

            return list;
        }
    }
}