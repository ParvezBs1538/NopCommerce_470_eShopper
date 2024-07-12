using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

public class SmartCarouselCategoryBuilder : NopEntityBuilder<SmartCarouselCategoryMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(SmartCarouselCategoryMapping.CarouselId)).AsInt32().ForeignKey<SmartCarousel>()
             .WithColumn(nameof(SmartCarouselCategoryMapping.CategoryId)).AsInt32().ForeignKey<Category>();
    }
}
