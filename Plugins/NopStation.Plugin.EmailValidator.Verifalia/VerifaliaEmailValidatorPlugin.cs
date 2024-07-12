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

namespace NopStation.Plugin.EmailValidator.Verifalia
{
    public class VerifaliaEmailValidatorPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public VerifaliaEmailValidatorPlugin(IPermissionService permissionService,
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
            return $"{_webHelper.GetStoreLocation()}Admin/VerifaliaEmailValidator/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new VerifaliaEmailValidatorSettings();
            settings.RevalidateInvalidEmailsAfterHours = 70;
            await _settingService.SaveSettingAsync(settings, x => x.RevalidateInvalidEmailsAfterHours, 0, true);

            await this.InstallPluginAsync(new VerifaliaEmailValidatorPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new VerifaliaEmailValidatorPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Menu.VerifaliaEmailValidator")
            };

            if (await _permissionService.AuthorizeAsync(VerifaliaEmailValidatorPermissionProvider.ManageVerifalia))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/VerifaliaEmailValidator/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Menu.Configuration"),
                    SystemName = "VerifaliaEmailValidator.Configuration"
                };
                menu.ChildNodes.Add(settings);

                var emails = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/VerifaliaEmailValidator/EmailList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VerifaliaEmailValidator.Menu.Emails"),
                    SystemName = "VerifaliaEmailValidator.Emails"
                };
                menu.ChildNodes.Add(emails);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/verifalia-email-validator-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=verifalia-email-validator",
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
                ["Admin.NopStation.VerifaliaEmailValidator.Menu.VerifaliaEmailValidator"] = "Email validator (Verifalia)",
                ["Admin.NopStation.VerifaliaEmailValidator.Menu.Configuration"] = "Configuration",
                ["Admin.NopStation.VerifaliaEmailValidator.Menu.Emails"] = "Emails",

                ["Admin.NopStation.VerifaliaEmailValidator.Configuration"] = "Verifalia email validator settings",
                ["Admin.NopStation.VerifaliaEmailValidator.ValidtaedEmails"] = "Validtaed emails",

                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.EnablePlugin"] = "Enable plugin",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.EnablePlugin.Hint"] = "Determines whether the plugin is enabled or not.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateCustomerInfoEmail"] = "Customer info (or register) page. Validate email",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateCustomerInfoEmail.Hint"] = "Customer info (or register) page. Determines whether to validate email.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateCustomerAddressEmail"] = "Customer address pages. Validate email",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateCustomerAddressEmail.Hint"] = "Customer address pages. Determines whether to validate email.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.Username"] = "Username",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.Username.Hint"] = "Enter Verifalia username.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.Password"] = "Password",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.Password.Hint"] = "Enter Verifalia password.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.EnableLog"] = "Enable log",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.EnableLog.Hint"] = "Check to enable log.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateQuality"] = "Validate quality",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateQuality.Hint"] = "Check to enable quality of email validation.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.QualityLevel"] = "Quality level",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.QualityLevel.Hint"] = "Select quality level.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.AllowRiskyEmails"] = "Allow risky emails",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.AllowRiskyEmails.Hint"] = "Check to allow emails with 'Risk' classification.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.BlockedDomains"] = "Blocked domains (comma separated)",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.BlockedDomains.Hint"] = "Enter comma separated domain names which you want to block.",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.RevalidateInvalidEmailsAfterHours"] = "Revalidate invalid emails after hours",
                ["Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.RevalidateInvalidEmailsAfterHours.Hint"] = "Revalidate invalid emails after hours.",

                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchEmail"] = "Email",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchEmail.Hint"] = "Search by email.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchStatus"] = "Status",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchStatus.Hint"] = "Search by email status.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchClassification"] = "Classification",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchClassification.Hint"] = "Search by email classification.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchDisposable"] = "Disposable",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchDisposable.Hint"] = "Search by disposable email.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchFree"] = "Free",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchFree.Hint"] = "Search by free email.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchRoleAccount"] = "Role account",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchRoleAccount.Hint"] = "Search by role account email.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchCreatedFrom"] = "Created from",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchCreatedFrom.Hint"] = "Search by date created from.",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchCreatedTo"] = "Created to",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchCreatedTo.Hint"] = "Search by date created to.",

                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List"] = "Verifalia emails",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.DeleteSelected"] = "Delete selected",

                ["Admin.NopStation.VerifaliaEmailValidator.Common.All"] = "All",
                ["Admin.NopStation.VerifaliaEmailValidator.Common.Yes"] = "Yes",
                ["Admin.NopStation.VerifaliaEmailValidator.Common.No"] = "No",

                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Email"] = "Email",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Status"] = "Status",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Classification"] = "Classification",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.IsDisposable"] = "Disposable",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.IsFree"] = "Free",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.IsRoleAccount"] = "Role account",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.CreatedOn"] = "Created on",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.UpdatedOn"] = "Updated on",
                ["Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.ValidationCount"] = "Validation count",

                ["NopStation.VerifaliaEmailValidator.EmailAddress.InvalidDomain"] = "Invalid email provider",
                ["NopStation.VerifaliaEmailValidator.EmailAddress.InvalidEmail"] = "Invalid email address",
            };

            return list.ToList();
        }

        #endregion
    }
}