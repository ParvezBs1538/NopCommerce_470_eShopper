using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Data;

public class SmartSliderItemBuilder : NopEntityBuilder<SmartSliderItem>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SmartSliderItem.SliderId)).AsInt32().ForeignKey<SmartSlider>();
    }
}
