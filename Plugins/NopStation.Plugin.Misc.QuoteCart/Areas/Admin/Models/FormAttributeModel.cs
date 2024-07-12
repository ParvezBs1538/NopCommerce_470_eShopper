using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormAttributeModel : BaseNopEntityModel, ILocalizedModel<FormAttributeLocalizedModel>
{
    #region Ctor

    public FormAttributeModel()
    {
        Locales = [];
        PredefinedFormAttributeValueSearchModel = new ();
        FormAttributeFormSearchModel = new ();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Description")]
    public string Description { get; set; }

    public IList<FormAttributeLocalizedModel> Locales { get; set; }

    public PredefinedFormAttributeValueSearchModel PredefinedFormAttributeValueSearchModel { get; set; }

    public FormAttributeFormSearchModel FormAttributeFormSearchModel { get; set; }

    #endregion
}

public partial record FormAttributeLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Description")]
    public string Description { get; set; }
}
