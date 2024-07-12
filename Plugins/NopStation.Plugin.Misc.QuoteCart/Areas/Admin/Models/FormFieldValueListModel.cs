using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormFieldValueListModel : BasePagedListModel<FormFieldValueModel>
{
    public int FormFieldId { get; set; }
}
