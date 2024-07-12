using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MPay24Service;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.MPay24.Services;
using NopStation.Plugin.Payments.MPay24.Domains;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Linq;
using Nop.Services.Orders;
using NopStation.Plugin.Payments.MPay24.Components;

namespace NopStation.Plugin.Payments.MPay24
{
    public class MPay24PaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly MPay24PaymentSettings _mPay24PaymentSettings;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public MPay24PaymentProcessor(IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            MPay24PaymentSettings mPay24PaymentSettings,
            IPaymentService paymentService,
            IPaymentOptionService paymentOptionService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _mPay24PaymentSettings = mPay24PaymentSettings;
            _paymentService = paymentService;
            _paymentOptionService = paymentOptionService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _mPay24PaymentSettings.AdditionalFee, false);
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var customValues = _paymentService.DeserializeCustomValues(postProcessPaymentRequest.Order);
            if (!customValues.TryGetValue(MPay24PaymentDefaults.ShortNameLabel, out var shortName))
                return;

            var option = await _paymentOptionService.GetPaymentOptionByShortNameAsync(shortName.ToString());
            if (option == null || !option.Active)
                return;

            var paymentType = option.PaymentType;
            var brand = option.Brand;

            var brandQueryString = string.IsNullOrEmpty(brand) ? "" : (" Brand =\"" + brand + "\""); //A.V 30/09/2016, if Brand is empty in the DB, do not pass this parameter to MPay24
            var url = _mPay24PaymentSettings.Sandbox ? "https://test.mpay24.com/app/bin/etpproxy_v14" : "https://www.mpay24.com/app/bin/etpproxy_v14";

            var storeLocation = _webHelper.GetStoreLocation();

            var basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var factory = new ChannelFactory<ETPChannel>(basicHttpBinding, new EndpointAddress(new Uri(url)));
            factory.Credentials.UserName.UserName = _mPay24PaymentSettings.SoapUsername;
            factory.Credentials.UserName.Password = _mPay24PaymentSettings.SoapPassword;
            var serviceProxy = factory.CreateChannel();

            var paymentReq = new SelectPaymentRequest()
            {
                merchantID = (uint)_mPay24PaymentSettings.MerchantId,
                mdxi = "<Order Style=\"font-size:15px;font-family:'barlowbold',arial,tahoma,sans-serif;background:#fff;color:#000\" LogoStyle=\"background:url(https://www.triumph-teileshop.de/images_0001/images/triumph-logo.png);background-repeat:no-repeat;margin:6px 0 0 4px;height:44px\" HeaderStyle=\"background: #000;height:73px;border:1px solid #bfbfbf;border-bottom:0px;border-radius:0px\" DropDownListsStyle=\"margin:3px 3px;padding:3px 0px 3px 6px;border-radius:0px\" InputFieldsStyle=\"margin:3px 3px;padding:3px 0px 3px 6px;border-radius:0px\" "
                + "PageHeaderStyle = \"background-color:#d31039;color:#fff;margin-top:0;border:1px solid #bfbfbf;border-radius:0px;padding:7px 7px;border-top:0;border-bottom:0\" PageCaptionStyle = \"color:#fff;font-weight:bold;height:7px;padding:0px 8px\" PageStyle = \"background-color:#f8f8f8;font:15px Arial;color:#000;border:1px solid #bfbfbf;border-radius:0px;border-top:0px\" ButtonsStyle=\"background-color:#d31039;color:#fff;padding:5px 20px;font-size:15px;cursor:pointer;font-weight:bold;border:none;height:30px;border-radius:0\"><Tid>" + postProcessPaymentRequest.Order.Id + "</Tid>"
                + "<TemplateSet CSSName=\"MODERN\"/>"
                + ((!string.IsNullOrEmpty(paymentType)) ? "<PaymentTypes Enable=\"true\"><Payment Type=\"" + paymentType + "\" " + brandQueryString + "/></PaymentTypes>" : "")
                + "<Price>"
                + string.Format(CultureInfo.InvariantCulture, "{0:0.00}", postProcessPaymentRequest.Order.OrderTotal)
                + "</Price><URL><Success>" + string.Format("{0}Plugins/PaymentMPay24/PDTHandler/?orderId={1}", storeLocation, postProcessPaymentRequest.Order.OrderGuid)
                + "</Success><Confirmation>" + string.Format("{0}Plugins/PaymentMPay24/IPNHandler/?orderId={1}", storeLocation, postProcessPaymentRequest.Order.OrderGuid)
                + "</Confirmation><Cancel>" + string.Format("{0}Plugins/PaymentMPay24/CancelOrder/?orderId={1}", storeLocation, postProcessPaymentRequest.Order.OrderGuid)
                + "</Cancel></URL></Order>"
            };

            var result = serviceProxy.SelectPaymentAsync(paymentReq).Result;

            // cleanup
            factory.Close();
            serviceProxy.Close();

            if (result.@out.status == status.OK)
            {
                _httpContextAccessor.HttpContext.Response.Redirect(result.@out.location);
            }
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var paymentOptions = await _paymentOptionService.GetAllMPay24PaymentOptionsAsync(
                active: true, storeId: _storeContext.GetCurrentStore().Id);

            return !paymentOptions.Any();
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

            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            if (!form.ContainsKey(MPay24PaymentDefaults.ShortName))
                return new List<string>() { await _localizationService.GetResourceAsync("NopStation.MPay24.PaymentOption.NotSelected") };

            var shortname = form[MPay24PaymentDefaults.ShortName].ToString();
            var paymentOption = await _paymentOptionService.GetPaymentOptionByShortNameAsync(shortname);

            if(paymentOption == null || !paymentOption.Active)
                return new List<string>() { await _localizationService.GetResourceAsync("NopStation.MPay24.PaymentOption.Invalid") };

            return new List<string>();
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var processPaymentRequest = new ProcessPaymentRequest();
            processPaymentRequest.CustomValues.Add(MPay24PaymentDefaults.ShortNameLabel, form[MPay24PaymentDefaults.ShortName].ToString());

            return Task.FromResult(processPaymentRequest);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentMPay24/Configure";
        }

        public override async Task InstallAsync()
        {
            #region Data Seed

            var paymentOptions = new List<PaymentOption>
            {
                new PaymentOption
                {
                    PaymentType = "CC",
                    Brand = "VISA",
                    DisplayName = "Visa",
                    ShortName = "visa",
                    DisplayOrder = 1,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "CC",
                    Brand = "MASTERCARD",
                    DisplayName = "MasterCard",
                    ShortName = "mastercard",
                    DisplayOrder = 2,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "MAESTRO",
                    DisplayName = "Maestro",
                    ShortName = "maestro",
                    DisplayOrder = 3,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "CC",
                    Brand = "AMEX",
                    DisplayName = "Amex",
                    ShortName = "amex",
                    DisplayOrder = 4,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "CC",
                    Brand = "DINERS",
                    DisplayName = "Diners Club",
                    ShortName = "diners_club",
                    DisplayOrder = 5,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "ELV",
                    Brand = "HOBEX-AT",
                    DisplayName = "Lastschrift (AT)",
                    ShortName = "lastschrift_(at)",
                    DisplayOrder = 6,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "ELV",
                    Brand = "HOBEX-DE",
                    DisplayName = "Lastschrift (DE)",
                    ShortName = "lastschrift_(de)",
                    DisplayOrder = 7,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "EPS",
                    Brand = "EPS",
                    DisplayName = "eps Online-Überweisung",
                    ShortName = "eps_online-uberweisung",
                    DisplayOrder = 8,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "PAYPAL",
                    Brand = "PAYPAL",
                    DisplayName = "Paypal",
                    ShortName = "paypal",
                    DisplayOrder = 9,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "GIROPAY",
                    Brand = "GIROPAY",
                    DisplayName = "Giropay",
                    ShortName = "giropay",
                    DisplayOrder = 10,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "SOFORT",
                    DisplayName = "SOFORT Überweisung",
                    ShortName = "sofort_uberweisung",
                    DisplayOrder = 11,
                    Active = true
                },
                new PaymentOption
                {
                    PaymentType = "INVOICE",
                    Brand = "INVOICE",
                    DisplayName = "Kauf auf Rechnung",
                    ShortName = "kauf_auf_rechnung",
                    DisplayOrder = 12,
                    Active = true
                },
            };
            await _paymentOptionService.InsertPaymentOptionAsync(paymentOptions);

            #endregion

            await this.InstallPluginAsync(new MPay24PaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<MPay24PaymentSettings>();
            await this.UninstallPluginAsync(new MPay24PaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentMPay24ViewComponent);
        }

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.MPay24.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Menu.MPay24", "mPAY24"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Menu.PaymentOptions", "Payment options"),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.CurrencySuggestion", "Please ensure that nopCommerce primary currency matches mPAY24 currency."),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchName", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchName.Hint", "Search by name"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchBrand", "Brand"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchBrand.Hint", "Search by brand."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchPaymentType", "Payment type"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchPaymentType.Hint", "Search by payment type."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchStoreId", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List.SearchStoreId.Hint", "Search by store."),

                new KeyValuePair<string, string>("NopStation.MPay24.PaymentMethodDescription", "Pay using MPay24 payment menthod"),
                new KeyValuePair<string, string>("NopStation.MPay24.PaymentOption.NotSelected", "Payment option is not selected."),
                new KeyValuePair<string, string>("NopStation.MPay24.PaymentOption.Invalid", "Invalid payment option."),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.PaymentType", "PaymentType"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.PaymentType.Hint", "Enter payment type."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Logo", "Logo"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Brand", "Brand"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Brand.Hint", "Enter brand."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayName", "Display name"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayName.Hint", "Enter display name."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.ShortName", "Short name"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.ShortName.Hint", "Enter short name."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.PictureId", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.PictureId.Hint", "Select picture."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Description.Hint", "Enter description."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Active.Hint", "Check to mark as active."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayOrder.Hint", "The display order for this payment option. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.SelectedStoreIds.Hint", "Option to limit this payment option to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.ShortNameAlreadyExists", "Short name already exists."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Created", "Payment option has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Updated", "Payment option has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Deleted", "Payment option has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.PaymentType.Required", "The payment type is required."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.DisplayName.Required", "The display name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.Brand.Required", "The brand is required."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.Fields.ShortName.Required", "The short name is required."),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.List", "Payment options"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.EditDetails", "Edit payment option details"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.AddNew", "Add new payment option"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.PaymentOptions.BackToList", "back to payment option list"),

                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration", "mPAY24 settings"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.SoapUsername", "Soap username"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.SoapUsername.Hint", "Enter soap username."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.SoapPassword", "Soap password"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.SoapPassword.Hint", "Enter soap password"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.Sandbox", "Sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.Sandbox.Hint", "Check to mark as sandbox mode."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.MerchantId", "Merchant id"),
                new KeyValuePair<string, string>("Admin.NopStation.MPay24.Configuration.Fields.MerchantId.Hint", "Enter merchant id."),
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.Menu.MPay24"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.Menu.Configuration"),
                    Url = "~/Admin/PaymentMPay24/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "MPay24.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);

                categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.Menu.PaymentOptions"),
                    Url = "~/Admin/PaymentMPay24/PaymentOptions",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "MPay24.PaymentOptions"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/mpay24-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=mpay24-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        #endregion
    }
}
