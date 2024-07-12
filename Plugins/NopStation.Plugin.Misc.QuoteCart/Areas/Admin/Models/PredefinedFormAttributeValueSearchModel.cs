using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record PredefinedFormAttributeValueSearchModel : BaseSearchModel
{
    #region Properties

    public int FormAttributeId { get; set; }

    #endregion
}