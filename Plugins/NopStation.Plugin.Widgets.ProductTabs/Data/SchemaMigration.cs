using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Data
{
    [NopMigration("2020/07/08 08:45:55:1687547", "NopStation.Plugin.Widgets.ProductTabs base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ProductTab>();
            Create.TableFor<ProductTabItem>();
            Create.TableFor<ProductTabItemProduct>();
        }
    }
}
