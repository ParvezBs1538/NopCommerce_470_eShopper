using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.OrderRatings.Domain;

namespace NopStation.Plugin.Widgets.OrderRatings.Data
{
    public class OrderRatingBuilder : NopEntityBuilder<OrderRating>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(OrderRating.OrderId)).AsInt32().ForeignKey<Order>().PrimaryKey();
        }
    }
}
