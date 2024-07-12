using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ExportImportTopic.Models
{
    public class ConfigurationModel
    {
        [NopResourceDisplayName("Admin.NopStation.ExportImportTopic.Configuration.Fields.CheckBodyMaximumLength")]
        public bool CheckBodyMaximumLength { get; set; }
        public bool CheckBodyMaximumLength_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ExportImportTopic.Configuration.Fields.BodyMaximumLength")]
        public int BodyMaximumLength { get; set; }
        public bool BodyMaximumLength_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
