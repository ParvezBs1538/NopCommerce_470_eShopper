using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    public record FlipbookModel : BaseNopEntityModel, IAclSupportedModel, ILocalizedModel<FlipbookLocalizedModel>, IStoreMappingSupportedModel
    {
        public FlipbookModel()
        {
            Locales = new List<FlipbookLocalizedModel>();
            AvailableFlipbooks = new List<SelectListItem>();
            FlipbookContentSearchModel = new FlipbookContentSearchModel();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.AvailableStartDateTime")]
        [UIHint("DateTimeNullable")]
        public DateTime? AvailableStartDateTime { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.AvailableEndDateTime")]
        [UIHint("DateTimeNullable")]
        public DateTime? AvailableEndDateTime { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        public IList<FlipbookLocalizedModel> Locales { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public FlipbookContentSearchModel FlipbookContentSearchModel { get; set; }

        public IList<SelectListItem> AvailableFlipbooks { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }

    public partial record FlipbookLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.Flipbooks.Fields.SeName")]
        public string SeName { get; set; }
    }
}
