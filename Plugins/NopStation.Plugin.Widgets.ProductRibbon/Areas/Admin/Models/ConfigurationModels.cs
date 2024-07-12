using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            BestSellPaymentStatusIds = new List<int>();
            BestSellOrderStatusIds = new List<int>();
            BestSellShippingStatusIds = new List<int>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone")]
        public string ProductDetailsPageWidgetZone { get; set; }
        public bool ProductDetailsPageWidgetZone_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone")]
        public string ProductOverviewBoxWidgetZone { get; set; }
        public bool ProductOverviewBoxWidgetZone_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableNewRibbon")]
        public bool EnableNewRibbon { get; set; }
        public bool EnableNewRibbon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableDiscountRibbon")]
        public bool EnableDiscountRibbon { get; set; }
        public bool EnableDiscountRibbon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableBestSellerRibbon")]
        public bool EnableBestSellerRibbon { get; set; }
        public bool EnableBestSellerRibbon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.SoldInDays")]
        public int SoldInDays { get; set; }
        public bool SoldInDays_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellStoreWise")]
        public bool BestSellStoreWise { get; set; }
        public bool BestSellStoreWise_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellPaymentStatus")]
        public IList<int> BestSellPaymentStatusIds { get; set; }
        public bool BestSellPaymentStatusIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellOrderStatus")]
        public IList<int> BestSellOrderStatusIds { get; set; }
        public bool BestSellOrderStatusIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellShippingStatus")]
        public IList<int> BestSellShippingStatusIds { get; set; }
        public bool BestSellShippingStatusIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumAmountSold")]
        public decimal MinimumAmountSold { get; set; }
        public bool MinimumAmountSold_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumQuantitySold")]
        public int MinimumQuantitySold { get; set; }
        public bool MinimumQuantitySold_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        public string CurrencyCode { get; set; }
    }
}