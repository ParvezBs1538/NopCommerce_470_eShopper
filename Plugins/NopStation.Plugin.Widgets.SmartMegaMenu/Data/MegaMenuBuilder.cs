using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data;

public class MegaMenuBuilder : NopEntityBuilder<MegaMenu>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(MegaMenu.Name)).AsString(400).NotNullable();

    }
}
