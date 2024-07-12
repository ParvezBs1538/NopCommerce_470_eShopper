using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.Product360View.Domain;

namespace NopStation.Plugin.Widgets.Product360View.Data
{
    [NopMigration("2023/08/23 00:00:00", "NopStation.Plugin.Widgets.Product360View base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ProductPictureMapping360>();
            Create.TableFor<ProductImageSetting360>();
        }
    }
}