using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.ShippingCountry.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableShippingCountry = new List<SelectListItem>();
        }
        public int DiscountId { get; set; }
        public int RequirementId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.DiscountRules.ShippingCountry.Fields.ShippingCountry")]
        public string ShippingCountry { get; set; }
        public IList<SelectListItem> AvailableShippingCountry { get; set; }
        public int DiscountTypeId { get; set; }
    }
}