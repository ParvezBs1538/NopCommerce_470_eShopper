using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class SearchFilterResult
    {
        public ResultPrice PriceRange { get; set; }
        public IList<Manufacturer> Manufacturers { get; set; }
        public IList<Vendor> Vendors { get; set; }
        public IList<ProductTag> ProductTags { get; set; }
        public IList<ProductAttribute> ProductAttributes { get; set; }
        public IList<Specification> Specifications { get; set; }
        public IList<Attribute> Attributes { get; set; }
        public IList<Rating> Ratings { get; set; }
        public IPagedList<Product> Products { get; set; }

        public SearchFilterResult()
        {
            PriceRange = new ResultPrice();
            Manufacturers = new List<Manufacturer>();
            Vendors = new List<Vendor>();
            ProductTags = new List<ProductTag>();
            Specifications = new List<Specification>();
            Attributes = new List<Attribute>();
            Ratings= new List<Rating>();
        }


        #region NestedClass
        public class ResultPrice
        {
            public int PriceMin { get; set; }
            public int PriceMax { get; set; }
        }


        public class Manufacturer
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }

        public class ProductTag
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }


        public class Vendor
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }

        public class Rating
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }

        public class Specification : BaseEntity, ILocalizedEntity
        {
            public int Count { get; set; }
            public string Name { get; set; }
            public int SpecificationAttributeId { get; set; }
            public string Color { get; set; }
        }

        public class ProductAttribute
        {
            public int Count { get; set; }
            public int ProductAttributeId { get; set; }
            public string Name { get; set; }
        }

        public class Attribute
        {
            public int Id { get; set; }
            public int Count { get; set; }
            public string Name { get; set; }
            public string Color { get; set; }
        }
        #endregion
    }
}
