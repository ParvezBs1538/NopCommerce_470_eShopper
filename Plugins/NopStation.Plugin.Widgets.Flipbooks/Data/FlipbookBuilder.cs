using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Data
{
    public class FlipbookBuilder : NopEntityBuilder<Flipbook>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Flipbook.Name)).AsString(400).Nullable();
        }
    }
}
