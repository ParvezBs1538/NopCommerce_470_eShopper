using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models
{
    public record FAQCategoryModel : BaseNopEntityModel, ILocalizedModel<FAQCategoryLocalizedModel>, IStoreMappingSupportedModel
    {
        public FAQCategoryModel()
        {
            Locales = new List<FAQCategoryLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.Published")]
        public bool Published { get; set; }

        public IList<FAQCategoryLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.SelectedStoreIds")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }

    public record FAQCategoryLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.Fields.Description")]
        public string Description { get; set; }
    }
}