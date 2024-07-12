using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Data
{
    public class ApiCategoryIconRecordBuilder : NopEntityBuilder<ApiCategoryIcon>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
              .WithColumn(nameof(ApiCategoryIcon.CategoryId)).AsInt32().ForeignKey<Category>()
              .WithColumn(nameof(ApiCategoryIcon.CategoryBannerId)).AsInt32();
        }
    }
}
