using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategorySEOTemplate
{
    public partial record CategorySEOTemplateModel : BaseNopEntityModel, IStoreMappingSupportedModel,
        ILocalizedModel<SEOTemplateLocalizedModel>
    {
        public CategorySEOTemplateModel()
        {
            //AvailableCategoryTitleTokens = new List<string>();
            AvailableCategoryMetaTitleTokens = new List<string>();
            AvailableCategoryMetaDescriptionTokens = new List<string>();
            AvailableCategoryMetaKeywordsTokens = new List<string>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            Locales = new List<SEOTemplateLocalizedModel>();

            CategoryCategorySEOTemplateMappingSearchModel = new CategoryCategorySEOTemplateMappingSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.TemplateName")]
        public string TemplateName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.TemplateRegex")]
        public string TemplateRegex { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.Priority")]
        public int Priority { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.ApplyToAllCategory")]
        public bool IsGlobalTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedByCustomer")]
        public int CreatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedByCustomer")]
        public string CreatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LastUpdatedByCustomer")]
        public int LastUpdatedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LastUpdatedByCustomer")]
        public string LastUpdatedByCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.IncludeProductNamesOnKeyword")]
        public bool IncludeProductNamesOnKeyword  { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.MaxNumberOfProductToInclude")]
        public int MaxNumberOfProductToInclude { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedOn")]
        //public DateTime CreatedOn { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.UpdatedOn")]
        //public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedOn")]
        public string CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.UpdatedOn")]
        public string UpdatedOn { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LimitedToStores")]
        //public IList<string> AvailableCategoryTitleTokens { get; set; }
        public List<string> AvailableCategoryMetaTitleTokens { get; set; }
        public List<string> AvailableCategoryMetaDescriptionTokens { get; set; }
        public List<string> AvailableCategoryMetaKeywordsTokens { get; set; }


        //store mapping
        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public CategoryCategorySEOTemplateMappingSearchModel CategoryCategorySEOTemplateMappingSearchModel { get; set; }

        #region SEO Template

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEOTitleTemplate")]
        public string SEOTitleTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEODescriptionTemplate")]
        public string SEODescriptionTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEOKeywordsTemplate")]
        public string SEOKeywordsTemplate { get; set; }
        
        public IList<SEOTemplateLocalizedModel> Locales { get; set; }

        #endregion
    }
}
