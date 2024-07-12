using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.FacebookMessenger
{
    public class FacebookMessengerSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string PageId { get; set; }

        public string ThemeColor { get; set; }

        public string Script { get; set; }

        public bool EnableScript { get; set; }
    }
}
