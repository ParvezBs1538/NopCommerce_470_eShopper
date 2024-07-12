using System.Collections.Generic;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Widgets.DMS.Models.Shippers
{
    public record ShipperLoginModel : LoginModel
    {
        public ShipperLoginModel()
        {
            StringResources = new List<KeyValueApi>();
        }
        public LanguageSelectorModel LanguageNavSelector { get; set; }

        public IList<KeyValueApi> StringResources { get; set; }

        public bool Rtl { get; set; }
    }
}
