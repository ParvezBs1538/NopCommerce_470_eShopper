using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AllInOneContactUs.Areas.Admin.Models
{
    public record WidgetsARContactUsConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableMessenger")]
        public bool EnableMessenger { get; set; }
        public bool EnableMessenger_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.MessengerId")]
        public string MessengerId { get; set; }
        public bool MessengerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableSkype")]
        public bool EnableSkype { get; set; }
        public bool EnableSkype_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.SkypeId")]
        public string SkypeId { get; set; }
        public bool SkypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTeams")]
        public bool EnableTeams { get; set; }
        public bool EnableTeams_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TeamsId")]
        public string TeamsId { get; set; }
        public bool TeamsId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableEmail")]
        public bool EnableEmail { get; set; }
        public bool EnableEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EmailId")]
        public string EmailId { get; set; }
        public bool EmailId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableCall")]
        public bool EnableCall { get; set; }
        public bool EnableCall_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.PhoneNumber")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTawkChat")]
        public bool EnableTawkChat { get; set; }
        public bool EnableTawkChat_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TawkChatSrc")]
        public string TawkChatSrc { get; set; }
        public bool TawkChatSrc_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableWhatsapp")]
        public bool EnableWhatsapp { get; set; }
        public bool EnableWhatsapp_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.WhatsappNumber")]
        public string WhatsappNumber { get; set; }
        public bool WhatsappNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableDirectContactUs")]
        public bool EnableDirectContactUs { get; set; }
        public bool EnableDirectContactUs_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.ContactUsPageUrl")]
        public string ContactUsPageUrl { get; set; }
        public bool ContactUsPageUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableTelegram")]
        public bool EnableTelegram { get; set; }
        public bool EnableTelegram_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.TelegramName")]
        public string TelegramName { get; set; }
        public bool TelegramName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableViber")]
        public bool EnableViber { get; set; }
        public bool EnableViber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.ViberNumber")]
        public string ViberNumber { get; set; }
        public bool ViberNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.EnableMeetingLink")]
        public bool EnableMeetingLink { get; set; }
        public bool EnableMeetingLink_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.AllInOneContactUs.ConfigurationModel.MeetingLink")]
        public string MeetingLink { get; set; }
        public bool MeetingLink_OverrideForStore { get; set; }
    }
}
