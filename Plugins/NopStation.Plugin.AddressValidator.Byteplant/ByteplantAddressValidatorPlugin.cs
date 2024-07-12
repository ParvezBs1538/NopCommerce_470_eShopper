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
using System.Linq;

namespace NopStation.Plugin.AddressValidator.Byteplant
{
    public class ByteplantAddressValidatorPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public ByteplantAddressValidatorPlugin(IPermissionService permissionService,
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
            return $"{_webHelper.GetStoreLocation()}Admin/ByteplantAddressValidator/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new ByteplantAddressValidatorPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new ByteplantAddressValidatorPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ByteplantAddressValidator.Menu.ByteplantAddressValidator")
            };

            if (await _permissionService.AuthorizeAsync(ByteplantAddressValidatorPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/ByteplantAddressValidator/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ByteplantAddressValidator.Menu.Configuration"),
                    SystemName = "ByteplantAddressValidator.Configuration"
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
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Menu.ByteplantAddressValidator", "Address validator (Byteplant)"),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration", "Byteplant address validator settings"),

                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.ByteplantApiKey", "Byteplant api key"),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.ByteplantApiKey.Hint", "Enter Byteplant map API key."),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.EnableLog", "Enable log"),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.EnableLog.Hint", "Check to enable log."),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.ValidatePhoneNumber", "Validate phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.ValidatePhoneNumber.Hint", "Check to enable phone number validation."),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"),
                new KeyValuePair<string, string>("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.PhoneNumberRegex.Hint", "Enter phone number regular expression."),

                new KeyValuePair<string, string>("NopStation.ByteplantAddressValidator.Checkout.InvalidAddress", "Invalid address"),
                new KeyValuePair<string, string>("NopStation.ByteplantAddressValidator.Customer.InvalidAddress", "Invalid address"),
                new KeyValuePair<string, string>("NopStation.ByteplantAddressValidator.Common.InvalidPhoneNumber", "Invalid phone number")
            };

            return list;
        }

        #endregion
    }
}