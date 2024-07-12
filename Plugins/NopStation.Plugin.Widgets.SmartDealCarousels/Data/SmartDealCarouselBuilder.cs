using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Data;

public class SmartDealCarouselBuilder : NopEntityBuilder<SmartDealCarousel>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SmartDealCarousel.Name))
            .AsString(400);
    }
}