using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.MegaMenu.Domains;

namespace NopStation.Plugin.Widgets.MegaMenu.Data;

public class CategoryIconRecordBuilder : NopEntityBuilder<CategoryIcon>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
          .WithColumn(nameof(CategoryIcon.CategoryId))
          .AsInt32()
          .WithColumn(nameof(CategoryIcon.PictureId))
          .AsInt32();
    }
}
