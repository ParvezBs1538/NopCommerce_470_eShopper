using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Data
{
    public class FlipbookContentBuilder : NopEntityBuilder<FlipbookContent>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FlipbookContent.FlipbookId)).AsInt32().ForeignKey<Flipbook>()
                .WithColumn(nameof(FlipbookContent.RedirectUrl)).AsString(400).Nullable();
        }
    }
}
