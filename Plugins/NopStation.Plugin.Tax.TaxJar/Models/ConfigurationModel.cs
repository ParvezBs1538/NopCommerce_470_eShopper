using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Tax.TaxJar.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableTaxJarApiVersions = new List<SelectListItem>();
            TaxJarTransactionLogSearchModel = new TaxJarTransactionLogSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.Token")]
        public string Token { get; set; }
        public bool Token_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.Country")]
        public int CountryId { get; set; }
        public bool CountryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.Zip")]
        public string Zip { get; set; }
        public bool Zip_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.City")]
        public string City { get; set; }
        public bool City_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.Street")]
        public string Street { get; set; }
        public bool Street_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.State")]
        public int StateId { get; set; }
        public bool StateId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.UseSandBox")]
        public bool UseSandBox { get; set; }
        public bool UseSandBox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.DisableItemWiseTax")]
        public bool DisableItemWiseTax { get; set; }
        public bool DisableItemWiseTax_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.AppliedOnCheckOutOnly")]
        public bool AppliedOnCheckOutOnly { get; set; }
        public bool AppliedOnCheckOutOnly_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.DefaultTaxCategory")]
        public int DefaultTaxCategoryId { get; set; }
        public bool DefaultTaxCategoryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.DisableTaxSubmit")]
        public bool DisableTaxSubmit { get; set; }
        public bool DisableTaxSubmit_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TaxJar.Configuration.Fields.TaxJarApiVersion")]
        public int TaxJarApiVersionId { get; set; }
        public bool TaxJarApiVersionId_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableTaxJarApiVersions { get; set; }
        public TaxJarTransactionLogSearchModel TaxJarTransactionLogSearchModel { get; set; }
    }
}
