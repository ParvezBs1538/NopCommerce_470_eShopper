using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record AffiliateCustomerModel : BaseNopEntityModel
    {
        public AffiliateCustomerModel()
        {
            AvailableApplyStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Affiliate")]
        public int AffiliateId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Affiliate")]
        public string AffiliateName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CustomerFullName")]
        public string CustomerFullName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.ApplyStatus")]
        public int ApplyStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.ApplyStatus")]
        public string ApplyStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.OverrideCatalogCommission")]
        public bool OverrideCatalogCommission { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CommissionAmount")]
        public decimal CommissionAmount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CommissionPercentage")]
        public decimal CommissionPercentage { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        public IList<SelectListItem> AvailableApplyStatuses { get; set; }
    }
}
