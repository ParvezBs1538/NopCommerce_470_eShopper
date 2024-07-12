using System.Collections.Generic;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models
{
    public class AlgoliaOverviewModel
    {
        public AlgoliaOverviewModel()
        {
            Product = new ProductOverviewModel();
            FilterableCategories = new List<CategoryModel>();
            FilterableManufacturers = new List<ManufacturerModel>();
            FilterableVendor = new VendorModel();
            FilterableSpecifications = new List<SpecificationModel>();
            FilterableAttributes = new List<AttributeModel>();
            FilterableKeywords = new List<string>();
            ProductCombinations = new List<ProductCombinationOverviewModel>();
        }

        public ProductOverviewModel Product { get; set; }

        public string objectID { get; set; }

        public string AutoCompleteImageUrl { get; set; }

        public string GTIN { get; set; }

        public int Rating { get; set; }

        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public IList<CategoryModel> FilterableCategories { get; set; }

        public IList<ManufacturerModel> FilterableManufacturers { get; set; }

        public IList<SpecificationModel> FilterableSpecifications { get; set; }

        public IList<AttributeModel> FilterableAttributes { get; set; }

        public VendorModel FilterableVendor { get; set; }

        public IList<string> FilterableKeywords { get; set; }
        public IList<ProductCombinationOverviewModel> ProductCombinations { get; set; }

        public long CreatedOn { get; set; }


        public class CategoryModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string SeName { get; set; }
        }

        public class ManufacturerModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string SeName { get; set; }
        }

        public class VendorModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string SeName { get; set; }
        }

        public class SpecificationModel
        {
            public int OptionId { get; set; }

            public int SpecificationAttributeId { get; set; }

            public string SpecificationAttributeName { get; set; }

            public string ValueRaw { get; set; }

            public string ColorSquaresRgb { get; set; }

            public int AttributeTypeId { get; set; }

            public string SpecificationValueGroup { get; set; }

            public string OptionIdSpecificationId { get; set; }
        }

        public class AttributeModel
        {
            public int AttributeId { get; set; }

            public string AttributeName { get; set; }

            public string AttributeValue { get; set; }

            public string ColorSquaresRgb { get; set; }

            public string AttributeIdValueGroup { get; set; }
        }
        public class ProductCombinationOverviewModel
        {
            public int CombinationId { get; set; }
            public string Sku { get; set; }
            public string GTIN { get; set; }
        }
    }
}
