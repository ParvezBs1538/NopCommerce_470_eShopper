using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.CrawlerManager.Domain;

namespace NopStation.Plugin.Widgets.CrawlerManager.Data.Builders
{
    public class CrawlerBuilder : NopEntityBuilder<Crawler>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            //table
            //    .WithColumn(nameof(Crawler.CustomerId)).AsInt32().ForeignKey<Customer>();
        }

        #endregion
    }
}
