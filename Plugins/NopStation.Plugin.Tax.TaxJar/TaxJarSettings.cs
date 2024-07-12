using Nop.Core.Configuration;

namespace NopStation.Plugin.Tax.TaxJar
{
    public class TaxJarSettings : ISettings
    {
        public int CountryId { get; set; }

        public string Zip { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public int StateId { get; set; }

        public string Token { get; set; }

        public bool UseSandBox { get; set; }

        public int TaxJarApiVersionId { get; set; }

        public bool DisableItemWiseTax { get; set; }

        public bool AppliedOnCheckOutOnly { get; set; }

        public bool DisableTaxSubmit { get; set; }

        public int DefaultTaxCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the tax rate cache time in minutes
        /// </summary>
        public int TaxRateCacheTime { get; set; }
    }
}
