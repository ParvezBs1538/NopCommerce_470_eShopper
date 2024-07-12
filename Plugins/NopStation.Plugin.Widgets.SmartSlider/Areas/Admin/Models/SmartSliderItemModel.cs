using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;

public record SmartSliderItemModel : BaseNopEntityModel, ILocalizedModel<SliderItemLocalizedModel>
{
    public SmartSliderItemModel()
    {
        AvailableContentTypes = new List<SelectListItem>();
        AvailableLanguages = new List<SelectListItem>();
        Locales = new List<SliderItemLocalizedModel>();
    }
    public int SliderId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Content")]
    public string Content { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.RedirectUrl")]
    public string RedirectUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.ButtonText")]
    public string ButtonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.ShowCaption")]
    public bool ShowCaption { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.ContentType")]
    public int ContentTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.ContentType")]
    public string ContentTypeStr { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.DesktopPictureId")]
    [UIHint("Picture")]
    public int DesktopPictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.MobilePictureId")]
    [UIHint("Picture")]
    public int MobilePictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.VideoDownloadId")]
    [UIHint("Video")]
    public int VideoDownloadId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.EmbeddedLink")]
    public string EmbeddedLink { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Text")]
    public string Text { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Language")]
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Language")]
    public string LanguageName { get; set; }

    public IList<SliderItemLocalizedModel> Locales { get; set; }

    public IList<SelectListItem> AvailableContentTypes { get; set; }
    public IList<SelectListItem> AvailableLanguages { get; set; }
}

public class SliderItemLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.RedirectUrl")]
    public string RedirectUrl { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.ButtonText")]
    public string ButtonText { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.Items.Fields.Text")]
    public string Text { get; set; }
}
