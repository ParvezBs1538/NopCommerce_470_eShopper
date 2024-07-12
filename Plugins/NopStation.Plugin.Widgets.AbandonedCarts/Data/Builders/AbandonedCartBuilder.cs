using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Data.Builders
{
    public class AbandonedCartBuilder : NopEntityBuilder<AbandonedCart>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AbandonedCart.CustomerId)).AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(AbandonedCart.ProductId)).AsInt32().ForeignKey<Product>();
        }

        #endregion
    }
}