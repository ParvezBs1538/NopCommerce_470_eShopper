using System.Collections.Concurrent;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public PublicInfoModel()
        {
            FilterProductsModel = new FilterProductsModel();
            FilterStockModel = new FilterStockModel();
            FilterPriceModel = new FilterPriceRangeModel();
            ManufacturerModel = new FilterManufacturersModel();
            ProductTagsModel = new FilterProductTagsModel();
            SpecificationModel = new FilterSpecificationsModel();
            VendorsModel = new FilterVendorsModel();
            AttributesModel = new FilterProductAttributesModel();
            FilterRatingModel = new FilterRatingModel();
            RequestParams = new ConcurrentDictionary<string, string>();
            RouteValues = new ConcurrentDictionary<string, string>();
            AjaxFilterParentCategoryModel = new AjaxFilterParentCategoryModel();
            AvailableSpecificationAttributeForCategoryId = new List<AjaxFilterSpecificationAttribute>();
        }
        public ProductsModel AjaxProductsModel { get; set; }
        public int CategoryId { get; set; }
        public int ManufacturerId { get; set; }
        public int VendorId { get; set; }

        public bool EnableFilter { get; set; }
        public FilterPriceRangeModel FilterPriceModel { get; set; }
        public FilterManufacturersModel ManufacturerModel { get; set; }
        public FilterProductTagsModel ProductTagsModel { get; set; }
        public FilterSpecificationsModel SpecificationModel { get; set; }
        public FilterVendorsModel VendorsModel { get; set; }
        public FilterProductAttributesModel AttributesModel { get; set; }
        public FilterStockModel FilterStockModel { get; set; }
        public FilterProductsModel FilterProductsModel { get; set; }
        public FilterRatingModel FilterRatingModel { get; set; }

        public int ViewMoreSpecId { get; set; }
        public string ViewMode { get; set; }
        public int PageSize { get; set; }
        public int SortOption { get; set; }
        public int PageNumber { get; set; }
        public int Count { get; set; }
        public string WidgetZone { get; set; }
        public string QuickViewButtonContainerName { get; set; }
        public IList<AjaxFilterSpecificationAttribute> AvailableSpecificationAttributeForCategoryId { get; set; }
        public string SearchElementName { get; set; }

        #region Filters

        public bool EnableProductAttributeFilter { get; set; }
        public string ProductAttributeOptionIds { get; set; }


        public bool EnablePriceRangeFilter { get; set; }
        public string FilteredPrice { get; set; }


        public bool EnableManufacturersFilter { get; set; }
        public string ManufacturerIds { get; set; }

        public bool EnableProductRating { get; set; }

        public bool EnableProductTagsFilter { get; set; }
        public string ProductTagIds { get; set; }

        public bool EnableVendorsFilter { get; set; }
        public string VendorIds { get; set; }


        public bool EnableSpecificationsFilter { get; set; }
        public string SpecificationOptionIds { get; set; }


        public bool EnableMiscFilter { get; set; }
        public bool FreeShipping { get; set; }
        public bool TaxExempt { get; set; }
        public bool DiscountedProduct { get; set; }
        public bool NewProduct { get; set; }
        public string ProductRatingIds { get; set; }
        public string ViewMoreOrShowLessClicked { get; set; }
        public int PageIndex { get; set; }
        public string SelectedSpecificationAttributes { get; set; }
        public string SelectedSpecificationAttributeOptions { get; set; }


        #endregion

        public AjaxFilterParentCategoryModel AjaxFilterParentCategoryModel { get; set; }

        #region Session Replaced Values

        public IDictionary<string, string> RouteValues { get; set; }
        public IDictionary<string, string> RequestParams { get; set; }
        public string RequestPath { get; set; }

        #endregion
    }
}
