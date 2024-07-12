using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record PredefinedFormAttributeValueModel : BaseNopEntityModel, ILocalizedModel<PredefinedFormAttributeValueLocalizedModel>
{
    #region Ctor

    public PredefinedFormAttributeValueModel()
    {
        Locales = [];
    }

    #endregion

    #region Properties

    public int FormAttributeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.IsPreSelected")]
    public bool IsPreSelected { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public IList<PredefinedFormAttributeValueLocalizedModel> Locales { get; set; }

    #endregion
}
public partial record PredefinedFormAttributeValueLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.Name")]
    public string Name { get; set; }
}