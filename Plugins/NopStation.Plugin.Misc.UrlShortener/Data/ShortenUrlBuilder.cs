using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.UrlShortener.Domains;

namespace NopStation.Plugin.Misc.UrlShortener.Data
{
    public class ShortenUrlBuilder : NopEntityBuilder<ShortenUrl>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.
                WithColumn(nameof(ShortenUrl.ShortUrl)).AsString(100);
        }
    }
}
