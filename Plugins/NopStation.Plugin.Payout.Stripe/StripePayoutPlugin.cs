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
using NopStation.Plugin.Payout.Stripe.Services;
using NopStation.Plugin.Widgets.VendorCommission.Domain;
using NopStation.Plugin.Widgets.VendorCommission.PayoutPluginManager;

namespace NopStation.Plugin.Payout.Stripe
{
    public class StripePayoutPlugin : BasePlugin, INopStationPlugin, IPayoutMethod, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly INopStationVendorCoreService _nopStationVendorCoreService;
        private readonly IWorkContext _workContext;
        private readonly IVendorStripePayoutService _vendorStripePayoutService;

        #endregion

        #region Ctor

        public StripePayoutPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            INopStationVendorCoreService nopStationVendorCoreService,
            IWorkContext workContext,
            IVendorStripePayoutService vendorStripePayoutService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _nopStationVendorCoreService = nopStationVendorCoreService;
            _workContext = workContext;
            _vendorStripePayoutService = vendorStripePayoutService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/StripePayout/Configure";
        }

        public string GetPayoutConfigurationUrl(int vendorId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("ConfigureVendorStripe", "StripePayout",
                new { vendorId = vendorId }, _webHelper.GetCurrentRequestProtocol());
        }
        public override async Task InstallAsync()
        {
            var settings = new StripePayoutSettings();
            await _settingService.SaveSettingAsync(settings);
            await this.InstallPluginAsync(new StripePayoutPermissionProvider());
            await base.InstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Payout.Menu.Stripe"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(StripePayoutPermissionProvider.ManageStripePayout))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Payout.Stripe.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/StripePayout/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StripePayout.Configuration"
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
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Active", "Active"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Active.Hint", "Check to activate plugin"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.UseSandBox", "Use sandbox"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.UseSandBox.Hint", "True for test."),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.SecretKey.Hint", "Secret key, provided by stripe."),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Instructions", "Instruction"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Configuration.Saved", "Configuration saved successfully"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Configuration.FailedToSave", "Failed to save configuration"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Configuration.Fields.AccountId", "Account id"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Configuration.Fields.AccountId.Hint", "Account id for payout"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Configuration.Fields.AccountId.Required", "Account id is required"),
                new KeyValuePair<string, string>("NopStation.Plugin.Payout.Stripe.Configuration.Save", "Save Stripe Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Payout.Menu.Stripe", "Stripe payout"),
                new KeyValuePair<string, string>("Admin.NopStation.Payout.Stripe.Menu.Configuration", "Configure"),
            };
        }
        public async Task<ProcessPayoutResult> ProcessPaymentAsync(ProcessPayoutRequest processPayoutRequest)
        {
            return await _vendorStripePayoutService.ProcessVendorPayoutAsync(processPayoutRequest);
        }
        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<StripePayoutSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("NopStation.Plugin.Payout.Stripe");
            await this.UninstallPluginAsync(new StripePayoutPermissionProvider());
            await base.UninstallAsync();
        }
        #endregion
    }
}