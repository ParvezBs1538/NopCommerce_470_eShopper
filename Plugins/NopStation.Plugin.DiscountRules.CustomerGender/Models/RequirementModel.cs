using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.CustomerGender.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableGender = new List<SelectListItem>();
        }
        public int DiscountId { get; set; }
        public int RequirementId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.DiscountRules.CustomerGender.Fields.Gender")]
        public string Gender { get; set; }
        public IList<SelectListItem> AvailableGender { get; set; }
    }
}