using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Data
{
    [NopMigration("2021/05/21 19:42:00", "NopStation.StoreLocator base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<StoreLocation>();
            Create.TableFor<StoreLocationPicture>();
        }
    }
}
