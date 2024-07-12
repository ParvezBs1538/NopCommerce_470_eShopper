using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;

public record CategoryIconModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category")]
    public int CategoryId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category")]
    public string CategoryName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture")]
    [UIHint("Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture")]
    public string PictureUrl { get; set; }
}
