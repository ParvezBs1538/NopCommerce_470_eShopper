using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Data
{
    [NopMigration("2021/06/10 08:50:55:1687541", "NopStation.AffiliateStation base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<AffiliateCustomer>();
            Create.TableFor<CatalogCommission>();
            Create.TableFor<OrderCommission>();
        }
    }
}