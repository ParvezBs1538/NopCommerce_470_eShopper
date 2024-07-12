using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Data
{
    [NopMigration("2022/07/29 09:47:55:1688542", "NopStation.AdvancedSEO base schema 4.60.1", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<CategorySEOTemplate>();
            Create.TableFor<ManufacturerSEOTemplate>();
            Create.TableFor<ProductSEOTemplate>();
            Create.TableFor<CategoryCategorySEOTemplateMapping>();
            Create.TableFor<ManufacturerManufacturerSEOTemplateMapping>();
            Create.TableFor<ProductProductSEOTemplateMapping>();
        }
    }
}
