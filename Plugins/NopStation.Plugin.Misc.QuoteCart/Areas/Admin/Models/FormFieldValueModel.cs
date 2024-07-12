using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public record FormFieldValueModel : BaseNopEntityModel, ILocalizedModel<FormFieldValueLocalizedModel>
{
    public FormFieldValueModel()
    {
        Locales = [];
    }

    public int FormFieldId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormFieldValue.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormFieldValue.Fields.ColorSquaresRgb")]
    public string ColorSquaresRgb { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormFieldValue.Fields.ImageSquaresPictureId")]
    [UIHint("Picture")]
    public int ImageSquaresPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormFieldValue.Fields.IsPreSelected")]
    public bool IsPreSelected { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormFieldValue.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }
    public IList<FormFieldValueLocalizedModel> Locales { get; set; }
}

public record FormFieldValueLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.FormFieldValue.Fields.Name")]
    public string Name { get; set; }
}
