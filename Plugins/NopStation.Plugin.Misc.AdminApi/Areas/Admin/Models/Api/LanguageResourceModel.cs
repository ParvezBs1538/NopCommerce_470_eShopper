using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

public record LanguageResourceModel : BaseNopModel
{
    public LanguageResourceModel()
    {
        StringResources = [];
    }

    public LanguageSelectorModel LanguageNavSelector { get; set; }

    public List<KeyValuePair<string, string>> StringResources { get; set; }

    public bool Rtl { get; set; }
}
