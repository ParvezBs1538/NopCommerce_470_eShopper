using Nop.Core;

namespace NopStation.Plugin.Widget.BlogNews.Domains;

public partial class BlogNewsPicture : BaseEntity
{
    public int PictureId { get; set; }

    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }

    public bool ShowInStore { get; set; }

    public EntityType EntityType
    {
        get => (EntityType)EntityTypeId;
        set => EntityTypeId = (int)value;
    }
}
