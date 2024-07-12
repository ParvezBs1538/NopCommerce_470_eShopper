using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.VendorCore.Services;
using NopStation.Plugin.Payout.PayPal.Services;
using NopStation.Plugin.Widgets.VendorCommission.Domain;
using NopStation.Plugin.Widgets.VendorCommission.PayoutPluginManager;

namespace NopStation.Plugin.Payout.PayPal
{
    public class PayPalPayoutPlugin : BasePlugin, INopStationPlugin, IPayoutMethod,IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly INopStationVendorCoreService _nopStationVendorCoreService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IVendorPayPalPayoutService _vendorPayPalPayoutService;

        #endregion

        #region Ctor

        public PayPalPayoutPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPermissionService permissionService,
            IWorkContext workContext,
            INopStationVendorCoreService nopStationVendorCoreService,
            INopStationCoreService nopStationCoreService,
            IVendorPayPalPayoutService vendorPayPalPayoutService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _permissionService = permissionService;
            _workContext = workContext;
            _nopStationVendorCoreService = nopStationVendorCoreService;
            _nopStationCoreService = nopStationCoreService;
            _vendorPayPalPayoutService = vendorPayPalPayoutService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/PayPalPayout/Configure";
        }
        public string GetPayoutConfigurationUrl(int vendorId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("ConfigureVendorPayPal", "PayPalPayout",
                new { vendorId = vendorId }, _webHelper.GetCurrentRequestProtocol());
        }
        public override async Task InstallAsync()
        {
            var settings = new PayPalPayoutSettings();
            await _settingService.SaveSettingAsync(settings);
            await this.InstallPluginAsync(new PayPalPayoutPermissionProvider());
            await base.InstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Payout.Menu.Paypal"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PayPalPayoutPermissionProvider.ManagePayPalPayout))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Payout.Paypal.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PayPalPayout/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PaypalPayout.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            if (await _workContext.GetCurrentVendorAsync() != null)
                await _nopStationVendorCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Active", "Active"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Active.Hint", "Check to activate plugin"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.UseSandBox", "Use sandbox"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.UseSandBox.Hint", "True for test."),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.ClientId", "Client Id"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.ClientId.Hint", "Client id, provided by PayPal."),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.SecretKey.Hint", "Secret key, provided by PayPal."),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Instructions", "Instruction"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Configuration.Saved", "Configuration saved successfully"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Configuration.FailedToSave", "Failed to save configuration"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Configuration.Fields.PayPalEmail", "PayPal email"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Configuration.Fields.PayPalEmail.Hint", "PayPal email for payout"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Configuration.Fields.PayPalEmail.Required", "PayPal email is required"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.PayPal.Configuration.Save", "Save PayPal Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Payout.Menu.Paypal", "Paypal payout"),
                new KeyValuePair<string, string>("Admin.NopStation.Payout.Paypal.Menu.Configuration", "Configure"),
            };
        }

        public async Task<ProcessPayoutResult> ProcessPaymentAsync(ProcessPayoutRequest processPayoutRequest)
        {
            var result = await _vendorPayPalPayoutService.ProcessPayPalPayoutAsync(processPayoutRequest);
            return result;
        }
        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<PayPalPayoutSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("NopStation.Plugin.Payout.PayPal");
            await this.UninstallPluginAsync(new PayPalPayoutPermissionProvider());
            await base.UninstallAsync();
        }

        #endregion
    }
}