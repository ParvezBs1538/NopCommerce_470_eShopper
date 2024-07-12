using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AllInOneContactUs.Components;
using NopStation.Plugin.Widgets.Announcement;

namespace NopStation.Plugin.Widgets.AllInOneContactUs
{
    public class ARContactUsPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public ARContactUsPlugin(IWebHelper webHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.BodyEndHtmlTagBefore });
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetsARContactUs/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AllInOneContactUsPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AllInOneContactUsPermissionProvider());
            await base.UninstallAsync();
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WidgetsARContactUsViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.Menu")
            };

            if (await _permissionService.AuthorizeAsync(AllInOneContactUsPermissionProvider.ManageAllInOneContactUs))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WidgetsARContactUs/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.Menu.Configuration"),
                    SystemName = "AllInOneContactUs.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/all-in-one-contactus-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=all-in-one-contactus",
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
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.AllInOneContactUs.ButtonText", "Contact us"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs", "All in one contact us"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.Menu", "AIO Contact us"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.Configuration", "AIO Contact us settings"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.TrackingScript", "Live chat Script code from chat provider:"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.TrackingScript.Hint", "Paste the tracking code generated from chat provider"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.IncludeCustomerNameAndEmail", "Include name & email"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.IncludeCustomerNameAndEmail.Hint", "Include customer name & email to tawk."),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableMessenger", "Enable messenger"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableMessenger.Hint", "Enable facebook messenger."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.MessengerId", "Messenger username"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.MessengerId.Hint", "Your messenger username. It will be transformed to https://m.me/YOURID."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableSkype", "Enable skype"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableSkype.Hint", "Enable skype."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.SkypeId", "Skype name"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.SkypeId.Hint", "Your skype name. It will be transformed to skype:YOURID?chat. To get your skype name - https://support.skype.com/en/faq/fa10858/what-s-my-skype-name#:~:text=Where%20can%20I%20find%20my,is%20displayed%20in%20your%20profile."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableEmail", "Enable email"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableEmail.Hint", "Enable email."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EmailId", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EmailId.Hint", "Your email."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableCall", "Enable phone call"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableCall.Hint", "Enable phone call."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.PhoneNumber", "Phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.PhoneNumber.Hint", "Your phone number."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTawkChat", "Enable tawk chat"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTawkChat.Hint", "Enable live chat with owner."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TawkChatSrc", "Tawk chat source url"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TawkChatSrc.Hint", "Your tawk chat source url from script given by tawk. It may look like https://embed.tawk.to/SOMEKEY1/SOMEKEY2."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableWhatsapp", "Enable whatsapp"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableWhatsapp.Hint", "Enable whatsapp."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.WhatsappNumber", "Whatsapp number"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.WhatsappNumber.Hint", "Your whatsapp number. It will be transformed to https://web.whatsapp.com/send?l=en&amp;phone=YOURNUMBER."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableDirectContactUs", "Enable message to contact form"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableDirectContactUs.Hint", "Enable direct contact us."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.ContactUsPageUrl", "Contact us page url"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.ContactUsPageUrl.Hint", "Contact us page url."),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTelegram", "Enable telegram"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTelegram.Hint", "Your telegram name."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TelegramName", "Enable telegram"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TelegramName.Hint", "Your telegram name."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableViber", "Enable viber"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableViber.Hint", "Your viber number."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.ViberNumber", "Enable telegram"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.ViberNumber.Hint", "Your telegram name."),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTeams", "Enable microsoft teams"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTeams.Hint", "Check to enable microsoft teams."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TeamsId", "Teams account"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TeamsId.Hint", "Your teams account."),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableMeetingLink", "Enable meeting link"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableMeetingLink.Hint", "Check to enable meeting link."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.MeetingLink", "Meeting link"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.MeetingLink.Hint", "Your meeting link."),
            };
            return list;
        }

        #endregion

        #region Properties

        public bool HideInWidgetList => false;

        #endregion
    }
}
