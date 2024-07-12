using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Data
{
    public class ProductQnABuilder : NopEntityBuilder<ProductQnA>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(ProductQnA.ProductId)).AsInt32().ForeignKey<Product>()
                 .WithColumn(nameof(ProductQnA.CustomerId)).AsInt32().ForeignKey<Customer>();
        }
    }
}
