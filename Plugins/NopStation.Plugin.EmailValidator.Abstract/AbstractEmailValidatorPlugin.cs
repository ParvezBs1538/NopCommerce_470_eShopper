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
using Nop.Services.Configuration;
using System.Linq;

namespace NopStation.Plugin.EmailValidator.Abstract
{
    public class AbstractEmailValidatorPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public AbstractEmailValidatorPlugin(IPermissionService permissionService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AbstractEmailValidator/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new AbstractEmailValidatorSettings();
            settings.RevalidateInvalidEmailsAfterHours = 70;
            await _settingService.SaveSettingAsync(settings, x => x.RevalidateInvalidEmailsAfterHours, 0, true);

            await this.InstallPluginAsync(new AbstractEmailValidatorPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AbstractEmailValidatorPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Menu.AbstractEmailValidator")
            };

            if (await _permissionService.AuthorizeAsync(AbstractEmailValidatorPermissionProvider.ManageAbstract))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AbstractEmailValidator/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Menu.Configuration"),
                    SystemName = "AbstractEmailValidator.Configuration"
                };
                menu.ChildNodes.Add(settings);

                var emails = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AbstractEmailValidator/EmailList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AbstractEmailValidator.Menu.Emails"),
                    SystemName = "AbstractEmailValidator.Emails"
                };
                menu.ChildNodes.Add(emails);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/abstract-email-validator-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=abstract-email-validator",
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
            var list = new Dictionary<string, string>
            {
                ["Admin.NopStation.AbstractEmailValidator.Menu.AbstractEmailValidator"] = "Email validator (Abstract)",
                ["Admin.NopStation.AbstractEmailValidator.Menu.Configuration"] = "Configuration",
                ["Admin.NopStation.AbstractEmailValidator.Menu.Emails"] = "Emails",

                ["Admin.NopStation.AbstractEmailValidator.Configuration"] = "Abstract email validator settings",
                ["Admin.NopStation.AbstractEmailValidator.ValidtaedEmails"] = "Validtaed emails",

                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.EnablePlugin"] = "Enable plugin",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.EnablePlugin.Hint"] = "Determines whether the plugin is enabled or not.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateCustomerInfoEmail"] = "Customer info (or register) page. Validate email",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateCustomerInfoEmail.Hint"] = "Customer info (or register) page. Determines whether to validate email.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateCustomerAddressEmail"] = "Customer address pages. Validate email",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateCustomerAddressEmail.Hint"] = "Customer address pages. Determines whether to validate email.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ApiKey"] = "Api key",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ApiKey.Hint"] = "Enter Abstract unique Api key.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.EnableLog"] = "Enable log",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.EnableLog.Hint"] = "Check to enable log.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateQuality"] = "Validate quality",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateQuality.Hint"] = "Check to enable quality of email validation.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.QualityLevel"] = "Quality level",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.QualityLevel.Hint"] = "Select quality level.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.AllowRiskyEmails"] = "Allow risky emails",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.AllowRiskyEmails.Hint"] = "Check to allow emails with 'Risk' deliverability.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.BlockedDomains"] = "Blocked domains (comma separated)",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.BlockedDomains.Hint"] = "Enter comma separated domain names which you want to block.",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.RevalidateInvalidEmailsAfterHours"] = "Revalidate invalid emails after hours",
                ["Admin.NopStation.AbstractEmailValidator.Configuration.Fields.RevalidateInvalidEmailsAfterHours.Hint"] = "Revalidate invalid emails after hours.",

                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchEmail"] = "Email",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchEmail.Hint"] = "Search by email.",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchDeliverability"] = "Deliverability",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchDeliverability.Hint"] = "Search by email deliverability.",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchDisposable"] = "Disposable",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchDisposable.Hint"] = "Search by disposable email.",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchFree"] = "Free",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchFree.Hint"] = "Search by free email.",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchRoleAccount"] = "Role account",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchRoleAccount.Hint"] = "Search by role account email.",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchCreatedFrom"] = "Created from",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchCreatedFrom.Hint"] = "Search by date created from.",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchCreatedTo"] = "Created to",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchCreatedTo.Hint"] = "Search by date created to.",

                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.List"] = "Abstract emails",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.DeleteSelected"] = "Delete selected",

                ["Admin.NopStation.AbstractEmailValidator.Common.All"] = "All",
                ["Admin.NopStation.AbstractEmailValidator.Common.Yes"] = "Yes",
                ["Admin.NopStation.AbstractEmailValidator.Common.No"] = "No",

                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.Email"] = "Email",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.QualityScore"] = "Quality Score",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.Deliverability"] = "Deliverability",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.IsDisposable"] = "Disposable",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.IsFree"] = "Free",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.IsRoleAccount"] = "Role account",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.CreatedOn"] = "Created on",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.UpdatedOn"] = "Updated on",
                ["Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.ValidationCount"] = "Validation count",

                ["NopStation.AbstractEmailValidator.EmailAddress.InvalidDomain"] = "Invalid email provider",
                ["NopStation.AbstractEmailValidator.EmailAddress.InvalidEmail"] = "Invalid email address",
            };

            return list.ToList();
        }

        #endregion
    }
}