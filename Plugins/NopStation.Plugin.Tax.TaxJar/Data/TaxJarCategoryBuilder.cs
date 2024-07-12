using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Tax;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Data
{
    public class TaxJarCategoryBuilder : NopEntityBuilder<TaxJarCategory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TaxJarCategory.TaxCategoryId)).AsInt32().ForeignKey<TaxCategory>().PrimaryKey()
                .WithColumn(nameof(TaxJarCategory.TaxCode)).AsString(100).Unique();
        }
    }
}
