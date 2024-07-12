using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record AffiliateCustomerSearchModel : BaseSearchModel
    {
        public AffiliateCustomerSearchModel()
        {
            ApplyStatusIds = new List<int>();
            AvailableApplyStatuses = new List<SelectListItem>();
            AvailableActiveStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.AffiliateFirstName")]
        public string AffiliateFirstName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.AffiliateLastName")]
        public string AffiliateLastName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ApplyStatusIds")]
        public IList<int> ApplyStatusIds { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ActiveStatusId")]
        public int ActiveStatusId { get; set; }

        public IList<SelectListItem> AvailableApplyStatuses { get; set; }
        public IList<SelectListItem> AvailableActiveStatuses { get; set; }
    }
}
