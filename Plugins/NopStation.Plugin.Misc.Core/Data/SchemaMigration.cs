using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.Core.Domains;

namespace NopStation.Plugin.Misc.Core.Data;

[NopMigration("2021/07/25 09:55:55:1687542", "NopStation.Core base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<License>();
    }
}
