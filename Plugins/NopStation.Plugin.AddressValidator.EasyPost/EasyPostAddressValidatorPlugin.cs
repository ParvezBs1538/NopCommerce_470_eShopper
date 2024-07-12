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

namespace NopStation.Plugin.AddressValidator.EasyPost
{
    public class EasyPostAddressValidatorPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public EasyPostAddressValidatorPlugin(IPermissionService permissionService,
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
            return $"{_webHelper.GetStoreLocation()}Admin/EasyPostAddressValidator/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new EasyPostAddressValidatorPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new EasyPostAddressValidatorPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.EasyPostAddressValidator.Menu.EasyPostAddressValidator")
            };

            if (await _permissionService.AuthorizeAsync(EasyPostAddressValidatorPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/EasyPostAddressValidator/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.EasyPostAddressValidator.Menu.Configuration"),
                    SystemName = "EasyPostAddressValidator.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/easypost-address-validator-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=easypost-address-validator",
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
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Menu.EasyPostAddressValidator", "Address validator (EasyPost)"),
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration", "EasyPost address validator settings"),

                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EasyPostApiKey", "EasyPost api key"),
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EasyPostApiKey.Hint", "Enter EasyPost map API key."),
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EnableLog", "Enable log"),
                new KeyValuePair<string, string>("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EnableLog.Hint", "Check to enable log."),

                new KeyValuePair<string, string>("NopStation.EasyPostAddressValidator.Checkout.InvalidAddress", "Invalid address"),
                new KeyValuePair<string, string>("NopStation.EasyPostAddressValidator.Customer.InvalidAddress", "Invalid address"),
                new KeyValuePair<string, string>("NopStation.EasyPostAddressValidator.Common.InvalidPhoneNumber", "Invalid phone number")
            };

            return list;
        }

        #endregion
    }
}