using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Models;

public partial record LicenseModel : BaseNopModel
{
    [NopResourceDisplayName("Admin.NopStation.Core.License.LicenseString")]
    public string LicenseString { get; set; }
}
