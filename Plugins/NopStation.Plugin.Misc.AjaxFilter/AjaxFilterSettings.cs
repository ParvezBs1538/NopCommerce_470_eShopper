using System.Collections.Generic;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;

namespace NopStation.Plugin.Misc.AjaxFilter
{
    public record AjaxFilterSettings : BaseNopModel, ISettings
    {
        public bool EnableFilter { get; set; }

        public bool EnableProductCount { get; set; }
        public bool HideManufacturerProductCount { get; set; }

        public int MaxDisplayForCategories { get; set; }

        public int MaxDisplayForManufacturers { get; set; }

        public bool EnableSpecificationAttributeFilter { get; set; }

        public bool EnableProductAttributeFilter { get; set; }
        public bool CloseProductAttributeFilterByDefualt { get; set; }

        public bool EnableVendorsFilter { get; set; }
        public bool CloseVendorsFilterByDefualt { get; set; }

        public bool EnableManufacturerFilter { get; set; }
        public bool CloseManufactureFilterByDefualt { get; set; }

        public bool EnableProductRatingsFilter { get; set; }
        public bool CloseProductRatingsFilterByDefualt { get; set; }

        public bool EnableProductTagsFilter { get; set; }
        public bool CloseProductTagsFilterByDefualt { get; set; }

        public bool EnablePriceRangeFilter { get; set; }
        public bool ClosePriceRangeFilterByDefualt { get; set; }

        public bool EnableCategoryFilter { get; set; }
        public bool CloseCategoryFilterByDefualt { get; set; }

        public bool EnableMiscFilter { get; set; }
        public bool CloseMiscFilterByDefualt { get; internal set; }

        public bool EnableGeneralSpecifications { get; set; }

        public IList<SpecificationAttribute> SpecificationAttributes { get; set; }
        public int ManufacturerFilterDisplayTypeId { get; set; }
        public bool EnableVendorFilter { get; internal set; }
        public int VendorFilterDisplayTypeId { get; internal set; }
        public int SpecificationFilterDisplayTypeId { get; internal set; }


        public FiltersUI ProductAttributeFilterDisplayTypeId { get; internal set; }
    }
}
