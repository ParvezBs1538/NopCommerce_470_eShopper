using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Data
{
    [NopMigration("2021/05/11 11:34:55:1689952", "NopStation.IpFilter base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<IpBlockRule>();
            Create.TableFor<IpRangeBlockRule>();
            Create.TableFor<CountryBlockRule>();
        }
    }
}

