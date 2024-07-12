using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormAttributeValueSearchModel : BaseSearchModel
{
    #region Properties

    public int FormAttributeId { get; set; }

    public int FormAttributeMappingId { get; set; }

    #endregion
}
