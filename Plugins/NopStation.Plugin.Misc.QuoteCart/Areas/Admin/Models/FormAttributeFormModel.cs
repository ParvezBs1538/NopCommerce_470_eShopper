using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record FormAttributeFormModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.UsedByForms.Form")]
    public string QuoteFormName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.UsedByForms.Published")]
    public bool Active { get; set; }

    #endregion
}
