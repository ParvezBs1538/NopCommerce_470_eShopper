using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public record AjaxFilterParentCategoryModel : BaseNopEntityModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool EnablePriceRangeFiltering { get; set; }
        public bool EnableManufactureFiltering { get; set; }
        public bool EnableSpecificationAttributeFiltering { get; set; }
        public bool EnableVendorFiltering { get; set; }
        public bool EnableSearchForSpecifications { get; set; }
        public bool EnableSearchForManufacturers { get; set; }
    }
}
