using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

[NopMigration("2022/07/08 08:41:55:1687548", "NopStation.SmartCarousels base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<SmartCarousel>();
        Create.TableFor<SmartCarouselProductMapping>();
        Create.TableFor<SmartCarouselManufacturerMapping>();
        Create.TableFor<SmartCarouselCategoryMapping>();
        Create.TableFor<SmartCarouselVendorMapping>();
        Create.TableFor<SmartCarouselPictureMapping>();
    }
}
