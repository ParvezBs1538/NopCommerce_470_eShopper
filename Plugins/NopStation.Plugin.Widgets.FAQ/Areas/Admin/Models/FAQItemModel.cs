using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models
{
    public record FAQItemModel : BaseNopEntityModel, ILocalizedModel<FAQItemLocalizedModel>, IStoreMappingSupportedModel
    {
        public FAQItemModel()
        {
            Locales = new List<FAQItemLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            SelectedCategoryIds = new List<int>();
            AvailableFAQCategories = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.Question")]
        public string Question { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.Answer")]
        public string Answer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.Permalink")]
        public string Permalink { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.FAQTags")]
        public string FAQTags { get; set; }

        public string InitialFAQTags { get; set; }

        public IList<FAQItemLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.SelectedStoreIds")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.SelectedCategoryIds")]
        public IList<int> SelectedCategoryIds { get; set; }
        public IList<SelectListItem> AvailableFAQCategories { get; set; }
    }

    public record FAQItemLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.Question")]
        public string Question { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.Fields.Answer")]
        public string Answer { get; set; }
    }
}