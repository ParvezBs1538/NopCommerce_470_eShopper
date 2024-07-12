using Nop.Core;

namespace NopStation.Plugin.Misc.AjaxFilter.Domains
{
    public class AjaxFilterSpecificationAttribute : BaseEntity
    {
        public int SpecificationId { get; set; }
        public int MaxSpecificationAttributesToDisplay { get; set; }
        public bool CloseSpecificationAttributeByDefault { get; set; }
        public string AlternateName { get; set; }
        public int DisplayOrder { get; set; }
        public bool HideProductCount { get; set; }
    }
}
