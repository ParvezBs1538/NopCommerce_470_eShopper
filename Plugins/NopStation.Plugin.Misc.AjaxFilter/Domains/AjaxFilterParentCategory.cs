using Nop.Core;

namespace NopStation.Plugin.Misc.AjaxFilter.Domains
{
    public class AjaxFilterParentCategory : BaseEntity
    {
        public int CategoryId { get; set; }
        public bool EnablePriceRangeFiltering { get; set; }
        public bool EnableManufactureFiltering { get; set; }
        public bool EnableSpecificationAttributeFiltering { get; set; }
        public bool EnableVendorFiltering { get; set; }
        public bool EnableSearchForSpecifications { get; set; }
        public bool EnableSearchForManufacturers { get; set; }
    }
}
