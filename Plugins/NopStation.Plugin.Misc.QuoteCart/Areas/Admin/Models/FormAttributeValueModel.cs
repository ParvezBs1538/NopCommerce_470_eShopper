using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record FormAttributeValueModel : BaseNopEntityModel, ILocalizedModel<FormAttributeValueLocalizedModel>
{
    #region Ctor

    public FormAttributeValueModel()
    {
        Locales = [];
    }

    #endregion

    #region Properties

    public int FormAttributeMappingId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.ColorSquaresRgb")]
    public string ColorSquaresRgb { get; set; }

    public bool DisplayColorSquaresRgb { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.ImageSquaresPicture")]
    [UIHint("Picture")]
    public int ImageSquaresPictureId { get; set; }

    public bool DisplayImageSquaresPicture { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.IsPreSelected")]
    public bool IsPreSelected { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public IList<FormAttributeValueLocalizedModel> Locales { get; set; }

    #endregion
}

public partial record FormAttributeValueLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.Name")]
    public string Name { get; set; }
}
