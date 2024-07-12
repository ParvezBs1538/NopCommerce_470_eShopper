using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.DaysOfWeek.Models
{
    public record RequirementModel
    {
        public RequirementModel()
        {
            AvailableDaysOfWeeks = new List<SelectListItem>();
            DaysOfWeek = new List<int>();
        }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek")]
        public IList<int> DaysOfWeek { get; set; }

        public IList<SelectListItem> AvailableDaysOfWeeks { get; set; }
    }
}