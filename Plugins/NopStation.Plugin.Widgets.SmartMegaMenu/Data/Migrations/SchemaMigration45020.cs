using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations;

[NopMigration("2022-12-27 00:00:00", "4.50.2.0", MigrationProcessType.Update)]
public class SchemaMigration45020 : Migration
{
    public override void Up()
    {
        var menuTableName = NameCompatibilityManager.GetTableName(typeof(MegaMenu));
        var gridViewColumnName = "GridView";
        var viewTypeColumnName = "ViewTypeId";

        if (Schema.Table(menuTableName).Column(gridViewColumnName).Exists())
        {
            Alter.Column(gridViewColumnName).OnTable(menuTableName).AsInt32();
            Rename.Column(gridViewColumnName).OnTable(menuTableName).To(viewTypeColumnName);
        }

        var hasWidgetZoneMappingAppliedColumnName = "HasWidgetZoneMappingApplied";
        if (!Schema.Table(menuTableName).Column(hasWidgetZoneMappingAppliedColumnName).Exists())
            Alter.Table(menuTableName)
                .AddColumn(hasWidgetZoneMappingAppliedColumnName).AsBoolean().NotNullable().SetExistingRowsTo(true);

        var menuItemTableName = NameCompatibilityManager.GetTableName(typeof(MegaMenuItem));
        var rph = Schema.Table(menuItemTableName);
        if (!rph.Exists())
            Create.TableFor<MegaMenuItem>();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}
