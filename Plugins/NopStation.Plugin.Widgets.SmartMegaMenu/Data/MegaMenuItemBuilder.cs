using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data;

public class MegaMenuItemBuilder : NopEntityBuilder<MegaMenuItem>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(MegaMenuItem.MegaMenuId)).AsInt32().ForeignKey<MegaMenu>();
    }
}
