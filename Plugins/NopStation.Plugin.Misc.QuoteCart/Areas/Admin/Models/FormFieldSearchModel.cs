using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record FormFieldSearchModel : BaseSearchModel
{
    public int FormId { get; set; }
}
