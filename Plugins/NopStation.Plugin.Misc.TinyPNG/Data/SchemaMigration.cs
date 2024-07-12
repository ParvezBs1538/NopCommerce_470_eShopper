using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.TinyPNG.Domain;

namespace NopStation.Plugin.Misc.TinyPNG.Data
{
    [NopMigration("2021/12/15 16:30:17:6455422", "NopStation.TinyPNG base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<PictureInfo>();
        }
    }
}