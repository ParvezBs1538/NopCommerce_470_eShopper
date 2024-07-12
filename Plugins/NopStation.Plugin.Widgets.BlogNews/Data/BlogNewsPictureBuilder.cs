using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widget.BlogNews.Domains;

namespace NopStation.Plugin.Widget.BlogNews.Data;

public class BlogNewsPictureBuilder : NopEntityBuilder<BlogNewsPicture>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.
          WithColumn(nameof(BlogNewsPicture.ShowInStore))
          .AsBoolean();
    }
}
