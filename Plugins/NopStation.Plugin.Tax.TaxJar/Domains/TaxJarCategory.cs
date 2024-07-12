using Nop.Core;

namespace NopStation.Plugin.Tax.TaxJar.Domains
{
    public class TaxJarCategory : BaseEntity
    {
        public int TaxCategoryId { get; set; }

        public string TaxCode { get; set; }

        public string Description { get; set; }
    }
}
