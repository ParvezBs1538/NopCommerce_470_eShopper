using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Media;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Data
{
    public partial class StoreLocationPictureBuilder : NopEntityBuilder<StoreLocationPicture>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(StoreLocationPicture.PictureId)).AsInt32().ForeignKey<Picture>()
                .WithColumn(nameof(StoreLocationPicture.StoreLocationId)).AsInt32().ForeignKey<StoreLocation>();
        }
    }
}