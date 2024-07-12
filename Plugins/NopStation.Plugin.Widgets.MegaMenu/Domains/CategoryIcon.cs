using Nop.Core;

namespace NopStation.Plugin.Widgets.MegaMenu.Domains;

public partial class CategoryIcon : BaseEntity
{
    public int CategoryId { get; set; }

    public int PictureId { get; set; }
}