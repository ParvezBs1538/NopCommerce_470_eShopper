using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Migrations
{
    [NopMigration("2023/10/17 08:40:55:1787545", "NopStation.VendorShop base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<OCarousel>();
            Create.TableFor<OCarouselItem>();
            Create.TableFor<Slider>();
            Create.TableFor<SliderItem>();
            Create.TableFor<ProductTab>();
            Create.TableFor<ProductTabItem>();
            Create.TableFor<ProductTabItemProduct>();
            Create.TableFor<VendorProfile>();
            Create.TableFor<VendorSubscriber>();
            Create.TableFor<VendorFeatureMapping>();
        }
    }
}
