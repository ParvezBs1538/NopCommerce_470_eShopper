using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.AddressValidator.EasyPost.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableAddressAttributes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EasyPostApiKey")]
        public string EasyPostApiKey { get; set; }
        public bool EasyPostApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.EasyPostAddressValidator.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableAddressAttributes { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
