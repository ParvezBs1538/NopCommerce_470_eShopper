using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.UrlShortener.Domains;

namespace NopStation.Plugin.Misc.UrlShortener.Data
{
    [NopMigration("2022/07/19 08:40:55:1687541", "NopStation.UrlShortener schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ShortenUrl>();
        }
    }
}
