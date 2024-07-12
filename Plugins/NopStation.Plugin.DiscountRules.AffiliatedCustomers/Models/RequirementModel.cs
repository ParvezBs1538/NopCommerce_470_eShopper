using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.AffiliatedCustomers.Models
{
    public record RequirementModel
    {
        public RequirementModel()
        {
            AvailableAffiliates = new List<SelectListItem>();
        }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.Affiliate")]
        public int AffiliateId { get; set; }

        public IList<SelectListItem> AvailableAffiliates { get; set; }
    }
}