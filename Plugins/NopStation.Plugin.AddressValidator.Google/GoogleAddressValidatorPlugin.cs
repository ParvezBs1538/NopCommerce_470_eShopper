using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.AddressValidator.Google
{
    public class GoogleAddressValidatorPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public GoogleAddressValidatorPlugin(IPermissionService permissionService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/GoogleAddressValidator/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new GoogleAddressValidatorPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new GoogleAddressValidatorPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.GoogleAddressValidator.Menu.GoogleAddressValidator")
            };

            if (await _permissionService.AuthorizeAsync(GoogleAddressValidatorPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/GoogleAddressValidator/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.GoogleAddressValidator.Menu.Configuration"),
                    SystemName = "GoogleAddressValidator.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/google-address-validator-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=google-address-validator",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Menu.GoogleAddressValidator", "Address validator (Google)"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration", "Google address validator settings"),

                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.GoogleApiKey", "Google api key"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.GoogleApiKey.Hint", "Enter Google map API key."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.EnableLog", "Enable log"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.EnableLog.Hint", "Check to enable log."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.ValidatePhoneNumber", "Validate phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.ValidatePhoneNumber.Hint", "Check to enable phone number validation."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.PhoneNumberRegex.Hint", "Enter phone number regular expression."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.StreetNumberAttributeId", "Street No. attribute"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.StreetNumberAttributeId.Hint", "Select street number attribute."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.StreetNameAttributeId", "Street name attribute"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.StreetNameAttributeId.Hint", "Select street name attribute."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.PlotNumberAttributeId", "Plot number"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.PlotNumberAttributeId.Hint", "Select plot number attribute."),

                new KeyValuePair<string, string>("NopStation.GoogleAddressValidator.Checkout.InvalidAddress", "Invalid address"),
                new KeyValuePair<string, string>("NopStation.GoogleAddressValidator.Customer.InvalidAddress", "Invalid address"),
                new KeyValuePair<string, string>("NopStation.GoogleAddressValidator.Common.InvalidPhoneNumber", "Invalid phone number")
            };

            return list;
        }

        #endregion
    }
}