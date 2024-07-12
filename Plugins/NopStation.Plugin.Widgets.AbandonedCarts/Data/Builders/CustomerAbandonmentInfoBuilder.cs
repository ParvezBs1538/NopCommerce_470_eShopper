using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Data.Builders
{
    public class CustomerAbandonmentInfoBuilder : NopEntityBuilder<CustomerAbandonmentInfo>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(CustomerAbandonmentInfo.CustomerId)).AsInt32().ForeignKey<Customer>();
        }

        #endregion
    }
}
