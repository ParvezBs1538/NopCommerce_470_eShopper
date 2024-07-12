using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvaliableSpecificationAttributes = new List<SelectListItem>();
            AjaxFilterSpecificationAttributeSearchModel = new AjaxFilterSpecificationAttributeSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableFilter")]
        public bool EnableFilter { get; set; }
        public bool EnableFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductCount")]
        public bool EnableProductCount { get; set; }
        public bool EnableProductCount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.HideManufacturerProductCount")]
        public bool HideManufacturerProductCount { get; set; }
        public bool HideManufacturerProductCount_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForCategories")]
        public int MaxDisplayForCategories { get; set; }
        public bool MaxDisplayForCategories_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForManufacturers")]
        public int MaxDisplayForManufacturers { get; set; }
        public bool MaxDisplayForManufacturers_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnablePriceRangeFilter")]
        public bool EnablePriceRangeFilter { get; set; }
        public bool EnablePriceRangeFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.ClosePriceRangeFilterByDefualt")]
        public bool ClosePriceRangeFilterByDefualt { get; set; }
        public bool ClosePriceRangeFilterByDefualt_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableCategoryFilter")]
        public bool EnableCategoryFilter { get; set; }
        public bool EnableCategoryFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseCategoryFilterByDefualt")]
        public bool CloseCategoryFilterByDefualt { get; set; }
        public bool CloseCategoryFilterByDefualt_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableSpecificationAttributeFilter")]
        public bool EnableSpecificationAttributeFilter { get; set; }
        public bool EnableSpecificationAttributeFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableManufacturerFilter")]
        public bool EnableManufacturerFilter { get; set; }
        public bool EnableManufacturerFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseManufactureFilterByDefualt")]
        public bool CloseManufactureFilterByDefualt { get; set; }
        public bool CloseManufactureFilterByDefualt_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductRatingsFilter")]
        public bool EnableProductRatingsFilter { get; set; }
        public bool EnableProductRatingsFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductRatingsFilterByDefualt")]
        public bool CloseProductRatingsFilterByDefualt { get; set; }
        public bool CloseProductRatingsFilterByDefualt_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductTagsFilter")]
        public bool EnableProductTagsFilter { get; set; }
        public bool EnableProductTagsFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductTagsFilterByDefualt")]
        public bool CloseProductTagsFilterByDefualt { get; set; }
        public bool CloseProductTagsFilterByDefualt_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductAttributeFilter")]
        public bool EnableProductAttributeFilter { get; set; }
        public bool EnableProductAttributeFilter_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductAttributeFilterByDefualt")]
        public bool CloseProductAttributeFilterByDefualt { get; set; }
        public bool CloseProductAttributeFilterByDefualt_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableMiscFilter")]
        public bool EnableMiscFilter { get; set; }
        public bool EnableMiscFilter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseMiscFilterByDefualt")]
        public bool CloseMiscFilterByDefualt { get; set; }
        public bool CloseMiscFilterByDefualt_OverrideForStore { get; set; }

        public AjaxFilterSpecificationAttributeSearchModel AjaxFilterSpecificationAttributeSearchModel { get; set; }

        public IList<SelectListItem> AvaliableSpecificationAttributes { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<int> SelectedSpecificationAttributeIds { get; set; }
    }
}
