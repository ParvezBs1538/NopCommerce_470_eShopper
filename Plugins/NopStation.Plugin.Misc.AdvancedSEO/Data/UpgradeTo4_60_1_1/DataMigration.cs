using FluentMigrator;
using Nop.Core;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Data.UpgradeTo4_60_1_1
{
    [NopMigration("2022/09/30 09:47:55:1687556", "NopStation.AdvancedSEO Migration for version: 4.60.1.1.1", MigrationProcessType.Update)]

    public class DataMigration : Migration
    {

        private readonly INopDataProvider _dataProvider;

        public DataMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public static string TableName<T>() where T : BaseEntity
        {
            return NameCompatibilityManager.GetTableName(typeof(T));
        }


        public override void Down()
        {
        }

        public override void Up()
        {
            //if (!Schema.Table(TableName<ProductSEOTemplate>()).Exists())
            //{
            //    Create.TableFor<ProductSEOTemplate>();
            //}
            //if (!Schema.Table(TableName<ProductProductSEOTemplateMapping>()).Exists())
            //{
            //    Create.TableFor<ProductProductSEOTemplateMapping>();
            //}
            //if (!Schema.Table(TableName<CategorySEOTemplate>()).Exists())
            //{
            //    Create.TableFor<CategorySEOTemplate>();
            //}
            //if (!Schema.Table(TableName<CategoryCategorySEOTemplateMapping>()).Exists())
            //{
            //    Create.TableFor<CategoryCategorySEOTemplateMapping>();
            //}
            //if (!Schema.Table(TableName<ManufacturerSEOTemplate>()).Exists())
            //{
            //    Create.TableFor<ManufacturerSEOTemplate>();
            //}
            //if (!Schema.Table(TableName<ManufacturerManufacturerSEOTemplateMapping>()).Exists())
            //{
            //    Create.TableFor<ManufacturerManufacturerSEOTemplateMapping>();
            //}

            if (!Schema.Table(TableName<ProductSEOTemplate>()).Column(nameof(ProductSEOTemplate.IncludeCategoryNamesOnKeyword)).Exists())
            {
                Alter.Table(TableName<ProductSEOTemplate>())
                    .AddColumn(nameof(ProductSEOTemplate.IncludeCategoryNamesOnKeyword)).AsBoolean().NotNullable().WithDefaultValue(0);
            }

            if (!Schema.Table(TableName<ProductSEOTemplate>()).Column(nameof(ProductSEOTemplate.IncludeProductTagsOnKeyword)).Exists())
            {
                Alter.Table(TableName<ProductSEOTemplate>())
                    .AddColumn(nameof(ProductSEOTemplate.IncludeProductTagsOnKeyword)).AsBoolean().NotNullable().WithDefaultValue(0);
            }

            if (!Schema.Table(TableName<ProductSEOTemplate>()).Column(nameof(ProductSEOTemplate.IncludeManufacturerNamesOnKeyword)).Exists())
            {
                Alter.Table(TableName<ProductSEOTemplate>())
                    .AddColumn(nameof(ProductSEOTemplate.IncludeManufacturerNamesOnKeyword)).AsBoolean().NotNullable().WithDefaultValue(0);
            }

            if (!Schema.Table(TableName<ProductSEOTemplate>()).Column(nameof(ProductSEOTemplate.IncludeVendorNamesOnKeyword)).Exists())
            {
                Alter.Table(TableName<ProductSEOTemplate>())
                    .AddColumn(nameof(ProductSEOTemplate.IncludeVendorNamesOnKeyword)).AsBoolean().NotNullable().WithDefaultValue(0);
            }

            if (!Schema.Table(TableName<CategorySEOTemplate>()).Column(nameof(CategorySEOTemplate.MaxNumberOfProductToInclude)).Exists())
            {
                Alter.Table(TableName<CategorySEOTemplate>())
                    .AddColumn(nameof(CategorySEOTemplate.MaxNumberOfProductToInclude)).AsInt32().NotNullable().WithDefaultValue(200);
            }

            if (!Schema.Table(TableName<ManufacturerSEOTemplate>()).Column(nameof(ManufacturerSEOTemplate.MaxNumberOfProductToInclude)).Exists())
            {
                Alter.Table(TableName<ManufacturerSEOTemplate>())
                    .AddColumn(nameof(ManufacturerSEOTemplate.MaxNumberOfProductToInclude)).AsInt32().NotNullable().WithDefaultValue(200);
            }
        }
    }
}
