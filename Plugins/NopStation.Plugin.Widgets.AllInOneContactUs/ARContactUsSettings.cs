using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.AllInOneContactUs
{
    public class ARContactUsSettings : ISettings
    {
        public bool EnableMessenger { get; set; }
        public string MessengerId { get; set; }

        public bool EnableSkype { get; set; }
        public string SkypeId { get; set; }

        public bool EnableTeams { get; set; }
        public string TeamsId { get; set; }

        public bool EnableEmail { get; set; }
        public string EmailId { get; set; }

        public bool EnableCall { get; set; }
        public string PhoneNumber { get; set; }

        public bool EnableTawkChat { get; set; }
        public string TawkChatSrc { get; set; }

        public bool EnableWhatsapp { get; set; }
        public string WhatsappNumber { get; set; }

        public bool EnableDirectContactUs { get; set; }
        public string ContactUsPageUrl { get; set; }

        public bool EnableTelegram { get; set; }
        public string TelegramName { get; set; }

        public bool EnableViber { get; set; }
        public string ViberNumber { get; set; }

        public bool EnableMeetingLink { get; set; }
        public string MeetingLink { get; set; }
    }
}