using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.MegaMenu.Models;

public record ManufacturerMenuModel : BaseNopEntityModel
{
    public ManufacturerMenuModel()
    {
        PictureModel = new PictureModel();
    }

    public string Name { get; set; }
    public string SeName { get; set; }
    public PictureModel PictureModel { get; set; }
}
