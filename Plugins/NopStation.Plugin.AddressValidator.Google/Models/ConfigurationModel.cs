using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.AddressValidator.Google.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableAddressAttributes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.GoogleApiKey")]
        public string GoogleApiKey { get; set; }
        public bool GoogleApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.ValidatePhoneNumber")]
        public bool ValidatePhoneNumber { get; set; }
        public bool ValidatePhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.StreetNumberAttributeId")]
        public int StreetNumberAttributeId { get; set; }
        public bool StreetNumberAttributeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.StreetNameAttributeId")]
        public int StreetNameAttributeId { get; set; }
        public bool StreetNameAttributeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.PlotNumberAttributeId")]
        public int PlotNumberAttributeId { get; set; }
        public bool PlotNumberAttributeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleAddressValidator.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableAddressAttributes { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
