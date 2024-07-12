using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormFieldValueSearchModel : BaseSearchModel
{
    public int FormFieldId { get; set; }
}
