using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class SearchModel
    {
        public SearchModel()
        {
            OrderBy = ProductSortingEnum.Position;
            CategoryIds = new List<int>();
            ManufacturerIds = new List<int>();
            StoreId = 0;
            VendorIds = new List<int>();
            WarehouseIds = new List<int>();
            LoadFilterableSpecificationAttributeOptionIds = false;
            PageIndex = 0;
            PageSize = int.MaxValue;
            ProductTagIds = new List<int>();
            ProductRatingIds = new List<int>();
            SearchDescriptions = false;
            SearchSku = true;
            SearchProductTags = false;
            LanguageId = 0;
            FilteredSpecs = new List<int>();
            ShowHidden = false;
            ParentGroupedProductId = 0;
            AllowedCustomerRolesIds = new List<int>();
            AttributeIds = new List<Attribute>();
            Url = "";
            AvaliableSpecificationAttributes = new List<AjaxFilterSpecificationAttribute>();
        }

        public string SelectedSpecificationAttributes { get; set; }
        public string SelectedSpecificationAttributeOptions { get; set; }
        public string SelectedAttributeOptions { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int FeaturedProducts { get; set; }
        public IList<int> ProductTagIds { get; set; }
        public IList<int> ProductRatingIds { get; set; }
        public string Keywords { get; set; }
        public bool SearchDescriptions { get; set; }
        public bool SearchSku { get; set; }
        public bool SearchProductTags { get; set; }
        public int LanguageId { get; set; }
        public IList<int> FilteredSpecs { get; set; }
        public string FilteredProductAttributes { get; set; }
        public bool ShowHidden { get; set; }
        public IList<int> AllowedCustomerRolesIds { get; set; }
        public string FilterBy { get; set; }
        public string Url { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool VisibleIndividuallyOnly { get; set; }
        public int ParentGroupedProductId { get; set; }
        public ProductSortingEnum OrderBy { get; set; }
        public ProductType? ProductType { get; set; }
        public IList<int> CategoryIds { get; set; }
        public IList<int> ManufacturerIds { get; set; }
        public int StoreId { get; set; }
        public IList<int> VendorIds { get; set; }
        public IList<int> WarehouseIds { get; set; }
        public bool OnlyInStock { get; set; }
        public bool NotInStock { get; set; }
        public int OnlyInStockQuantity { get; set; }

        //
        public bool FilterFreeShipping { get; set; }
        public bool FilterTaxExempt { get; set; }
        public bool FilterDiscountedProduct { get; set; }
        public bool FilterNewProduct { get; set; }
        public IList<Attribute> AttributeIds { get; set; }
        public bool LoadFilterableSpecificationAttributeOptionIds { get; set; }
        public IList<AjaxFilterSpecificationAttribute> AvaliableSpecificationAttributes { get; set; }
        public int ViewMoreSpecificationId { get; set; }
        public int MaxSpecificationAttributeToDisplayByDefault { get; set; }
    }
}
