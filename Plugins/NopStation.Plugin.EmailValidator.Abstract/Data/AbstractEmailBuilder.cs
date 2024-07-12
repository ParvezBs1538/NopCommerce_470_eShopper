using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.EmailValidator.Abstract.Domains;

namespace NopStation.Plugin.EmailValidator.Abstract.Data
{
    public class AbstractEmailBuilder : NopEntityBuilder<AbstractEmail>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(AbstractEmail.Email)).AsString(1000).NotNullable();
        }
    }
}