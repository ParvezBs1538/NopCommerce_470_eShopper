using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AllowedCustomerRolesIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.AllowedCustomerRolesIds")]
        public IList<int> AllowedCustomerRolesIds { get; set; }
        public bool AllowedCustomerRolesIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }
        public bool IncludeInTopMenu_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.IncludeInFooter")]
        public bool IncludeInFooter { get; set; }
        public bool IncludeInFooter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.FooterElementSelector")]
        public string FooterElementSelector { get; set; }
        public bool FooterElementSelector_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.LinkRequired")]
        public bool LinkRequired { get; set; }
        public bool LinkRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.DescriptionRequired")]
        public bool DescriptionRequired { get; set; }
        public bool DescriptionRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.Configuration.Fields.MinimumProductRequestCreateInterval")]
        public int MinimumProductRequestCreateInterval { get; set; }
        public bool MinimumProductRequestCreateInterval_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}