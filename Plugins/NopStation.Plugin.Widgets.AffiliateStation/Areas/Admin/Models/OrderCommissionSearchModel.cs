using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record OrderCommissionSearchModel : BaseSearchModel
    {
        #region Ctor

        public OrderCommissionSearchModel()
        {
            AvailableCommissionStatuses = new List<SelectListItem>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            CommissionStatusIds = new List<int>();
            OrderStatusIds = new List<int>();
            PaymentStatusIds = new List<int>();
        }

        #endregion

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.AffiliateFirstName")]
        public string AffiliateFirstName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.AffiliateLastName")]
        public string AffiliateLastName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.CommissionStatus")]
        public IList<int> CommissionStatusIds { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.OrderStatus")]
        public IList<int> OrderStatusIds { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.List.PaymentStatus")]
        public IList<int> PaymentStatusIds { get; set; }

        public IList<SelectListItem> AvailableCommissionStatuses { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
    }
}
