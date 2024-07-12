using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormAttributeMappingSearchModel : BaseSearchModel
{
    #region Properties

    public int FormId { get; set; }

    #endregion
}
