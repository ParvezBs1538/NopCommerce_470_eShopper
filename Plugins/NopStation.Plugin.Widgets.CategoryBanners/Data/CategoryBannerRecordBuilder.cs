using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.CategoryBanners.Domains;
using Nop.Data.Extensions;

namespace NopStation.Plugin.Widgets.CategoryBanners.Data
{
    public class CategoryBannerRecordBuilder : NopEntityBuilder<CategoryBanner>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CategoryBanner.CategoryId)).AsInt32().ForeignKey<Category>()
                .WithColumn(nameof(CategoryBanner.DisplayOrder)).AsInt32()
                .WithColumn(nameof(CategoryBanner.ForMobile)).AsBoolean()
                .WithColumn(nameof(CategoryBanner.PictureId)).AsInt32();
        }
    }
}