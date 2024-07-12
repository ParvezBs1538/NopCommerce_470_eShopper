using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Data
{
    [NopMigration("2020/06/08 08:30:55:1687541", "NopStation.WebApi base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ApiSlider>();
            Create.TableFor<ApiCategoryIcon>();
            Create.TableFor<ApiDevice>();
            Create.TableFor<ApiStringResource>();
        }
    }
}
