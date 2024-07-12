using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record OrderCommissionModel : BaseNopEntityModel
    {
        public OrderCommissionModel()
        {
            AvailableCommissionStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.TotalCommissionAmount")]
        public decimal TotalCommissionAmount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.TotalCommissionAmount")]
        public string TotalCommissionAmountStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionStatus")]
        public int CommissionStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionStatus")]
        public string CommissionStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionPaidOn")]
        [UIHint("DateTimeNullable")]
        public DateTime? CommissionPaidOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderStatus")]
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderStatus")]
        public string OrderStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PaymentStatus")]
        public int PaymentStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PaymentStatus")]
        public string PaymentStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PartialPaidAmount")]
        public decimal PartialPaidAmount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Affiliate")]
        public int AffiliateId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Affiliate")]
        public string AffiliateName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Customer")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Customer")]
        public string CustomerInfo { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        public IList<SelectListItem> AvailableCommissionStatuses { get; set; }
    }

    public record OrderCommissionAggreratorModel : BaseNopModel
    {
        public string aggregatortotal { get; set; }
        public string aggregatorpayable { get; set; }
        public string aggregatorpaid { get; set; }
    }
}
