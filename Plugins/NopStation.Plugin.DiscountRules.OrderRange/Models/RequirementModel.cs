using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.OrderRange.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableConditions = new List<SelectListItem>();
        }
        public int DiscountId { get; set; }
        public int RequirementId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue")]
        public string ConditionValue { get; set; }
        public IList<SelectListItem> AvailableConditions { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue")]
        public int RangeValue { get; set; }
    }
}