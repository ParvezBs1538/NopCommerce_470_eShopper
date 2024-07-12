using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductSEOTemplate
{
    public partial record ProductSEOTemplateModel : BaseNopEntityModel, IStoreMappingSupportedModel,
        ILocalizedModel<SEOTemplateLocalizedModel>
    {
        public ProductSEOTemplateModel()
        {
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            Locales = new List<SEOTemplateLocalizedModel>();

            //AvailableProductTitleTokens = new List<string>();
            AvailableProductMetaTitleTokens = new List<string>();
            AvailableProductMetaDescriptionTokens = new List<string>();
            AvailableProductMetaKeywordsTokens = new List<string>();

            ProductProductSEOTemplateMappingSearchModel = new ProductProductSEOTemplateMappingSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.TemplateName")]
        public string TemplateName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.TemplateRegex")]
        public string TemplateRegex { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.Priority")]
        public int Priority { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.ApplyToAllProduct")]
        public bool IsGlobalTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedByCustomer")]
        public int CreatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedByCustomer")]
        public string CreatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LastUpdatedByCustomer")]
        public int LastUpdatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LastUpdatedByCustomer")]
        public string LastUpdatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeProductTagsOnKeyword")]
        public bool IncludeProductTagsOnKeyword { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeCategoryNamesOnKeyword")]
        public bool IncludeCategoryNamesOnKeyword { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeManufacturerNamesOnKeyword")]
        public bool IncludeManufacturerNamesOnKeyword { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeVendorNamesOnKeyword")]
        public bool IncludeVendorNamesOnKeyword { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedOn")]
        //public DateTime CreatedOn { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.UpdatedOn")]
        //public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedOn")]
        public string CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.UpdatedOn")]
        public string UpdatedOn { get; set; }

        //public List<string> AvailableProductTitleTokens { get; set; }
        public List<string> AvailableProductMetaTitleTokens { get; set; }
        public List<string> AvailableProductMetaDescriptionTokens { get; set; }
        public List<string> AvailableProductMetaKeywordsTokens { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        #region SEO Template

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEOTitleTemplate")]
        public string SEOTitleTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEODescriptionTemplate")]
        public string SEODescriptionTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEOKeywordsTemplate")]
        public string SEOKeywordsTemplate { get; set; }
        
        public IList<SEOTemplateLocalizedModel> Locales { get; set; }

        #endregion



        public ProductProductSEOTemplateMappingSearchModel ProductProductSEOTemplateMappingSearchModel { get; set; }
    }
}
