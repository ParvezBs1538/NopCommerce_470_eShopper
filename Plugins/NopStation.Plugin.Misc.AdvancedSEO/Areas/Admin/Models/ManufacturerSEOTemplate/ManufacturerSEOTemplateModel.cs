using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerSEOTemplate
{
    public partial record ManufacturerSEOTemplateModel : BaseNopEntityModel, IStoreMappingSupportedModel,
        ILocalizedModel<SEOTemplateLocalizedModel>
    {
        public ManufacturerSEOTemplateModel()
        {
            //AvailableManufacturerTitleTokens = new List<string>();
            AvailableManufacturerMetaTitleTokens = new List<string>();
            AvailableManufacturerMetaDescriptionTokens = new List<string>();
            AvailableManufacturerMetaKeywordsTokens = new List<string>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            Locales = new List<SEOTemplateLocalizedModel>();
            ManufacturerManufacturerSEOTemplateMappingSearchModel = new ManufacturerManufacturerSEOTemplateMappingSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.TemplateName")]
        public string TemplateName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.TemplateRegex")]
        public string TemplateRegex { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.Priority")]
        public int Priority { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.ApplyToAllManufacturer")]
        public bool IsGlobalTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedByCustomer")]
        public int CreatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedByCustomer")]
        public string CreatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LastUpdatedByCustomer")]
        public int LastUpdatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LastUpdatedByCustomer")]
        public string LastUpdatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.IncludeProductNamesOnKeyword")]
        public bool IncludeProductNamesOnKeyword  { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.MaxNumberOfProductToInclude")]
        public int MaxNumberOfProductToInclude { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedOn")]
        //public DateTime CreatedOn { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.UpdatedOn")]
        //public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedOn")]
        public string CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.UpdatedOn")]
        public string UpdatedOn { get; set; }

        //public IList<string> AvailableManufacturerTitleTokens { get; set; }
        public List<string> AvailableManufacturerMetaTitleTokens { get; set; }
        public List<string> AvailableManufacturerMetaDescriptionTokens { get; set; }
        public List<string> AvailableManufacturerMetaKeywordsTokens { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public ManufacturerManufacturerSEOTemplateMappingSearchModel ManufacturerManufacturerSEOTemplateMappingSearchModel { get; set; }

        #region SEO Template

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEOTitleTemplate")]
        public string SEOTitleTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEODescriptionTemplate")]
        public string SEODescriptionTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEOKeywordsTemplate")]
        public string SEOKeywordsTemplate { get; set; }
        
        public IList<SEOTemplateLocalizedModel> Locales { get; set; }

        #endregion
    }
}
