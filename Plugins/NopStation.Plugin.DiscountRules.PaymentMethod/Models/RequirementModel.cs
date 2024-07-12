using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.PaymentMethod.Models
{
    public record RequirementModel
    {
        public RequirementModel()
        {
            AvailablePaymentMethods = new List<SelectListItem>();
        }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DiscountRules.PaymentMethod.Fields.PaymentMethod")]
        public string PaymentMethod { get; set; }

        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
    }
}