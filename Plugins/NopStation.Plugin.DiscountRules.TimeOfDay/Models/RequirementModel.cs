using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.TimeOfDay.Models
{
    public record RequirementModel
    {
        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [UIHint("Time")]
        [NopResourceDisplayName("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayFrom")]
        public DateTime TimeOfDayFrom { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayFrom")]
        public string TimeOfDayFromStr => TimeOfDayFrom.ToString("hh:mm tt");

        [UIHint("Time")]
        [NopResourceDisplayName("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo")]
        public DateTime TimeOfDayTo { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo")]
        public string TimeOfDayToStr => TimeOfDayTo.ToString("hh:mm tt");
    }
}