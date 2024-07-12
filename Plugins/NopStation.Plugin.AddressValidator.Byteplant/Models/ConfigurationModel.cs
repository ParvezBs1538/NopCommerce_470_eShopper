using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.AddressValidator.Byteplant.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableAddressAttributes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.ByteplantApiKey")]
        public string ByteplantApiKey { get; set; }
        public bool ByteplantApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.ValidatePhoneNumber")]
        public bool ValidatePhoneNumber { get; set; }
        public bool ValidatePhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ByteplantAddressValidator.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableAddressAttributes { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
