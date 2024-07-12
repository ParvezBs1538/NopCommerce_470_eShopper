using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselPictureModel : BaseNopEntityModel, ILocalizedModel<SmartCarouselPictureLocalizedModel>
{
    public SmartCarouselPictureModel()
    {
        Locales = new List<SmartCarouselPictureLocalizedModel>();
    }

    public int CarouselId { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture")]
    public string PictureUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Label")]
    public string Label { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.RedirectUrl")]
    public string RedirectUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.OverrideAltAttribute")]
    public string OverrideAltAttribute { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.OverrideTitleAttribute")]
    public string OverrideTitleAttribute { get; set; }

    public IList<SmartCarouselPictureLocalizedModel> Locales { get; set; }
}

public class SmartCarouselPictureLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Label")]
    public string Label { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.RedirectUrl")]
    public string RedirectUrl { get; set; }
}
