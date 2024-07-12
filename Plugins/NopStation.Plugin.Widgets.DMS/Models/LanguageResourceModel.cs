using System.Collections.Generic;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record LanguageResourceModel : BaseNopModel
    {
        public LanguageResourceModel()
        {
            StringResources = new List<KeyValueApi>();
        }
        public IList<KeyValueApi> StringResources { get; set; }
        public bool Rtl { get; set; }
    }
}
