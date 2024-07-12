using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductRequests.Domains;
using Nop.Data.Extensions;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.ProductRequests.Data
{
    public class ProductRequestBuilder : NopEntityBuilder<ProductRequest>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProductRequest.Name)).AsString(200)
                .WithColumn(nameof(ProductRequest.Link)).AsString(400).Nullable()
                .WithColumn(nameof(ProductRequest.CustomerId)).AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(ProductRequest.StoreId)).AsInt32().ForeignKey<Store>()
                .WithColumn(nameof(ProductRequest.Description)).AsString(500).Nullable();
        }
    }
}