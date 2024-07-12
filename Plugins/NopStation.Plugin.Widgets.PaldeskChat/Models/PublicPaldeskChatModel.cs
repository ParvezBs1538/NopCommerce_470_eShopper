using System;
using System.Collections.Generic;
using System.Text;

namespace NopStation.Plugin.Widgets.PaldeskChat.Models
{
    public class PublicPaldeskChatModel
    {
        public string Key { get; set; }

        public int SettingModeId { get; set; }

        public string Script { get; set; }

        public bool IsRegistered { get; set; }

        public bool ConfigureWithCustomerDataIfLoggedIn { get; set; }

        public Guid CustomerGuid { get; set; }

        public string CustomerUsername { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerFirstName { get; set; }

        public string CustomerLastName { get; set; }

        public string CustomerPhoneNumber { get; set; }
    }
}
