using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ProductRibbon
{
    public partial class ProductRibbonSettings : ISettings
    {
        public ProductRibbonSettings()
        {
            BestSellPaymentStatusIds = new List<int>();
            BestSellOrderStatusIds = new List<int>();
            BestSellShippingStatusIds = new List<int>();
        }

        public string ProductDetailsPageWidgetZone { get; set; }

        public string ProductOverviewBoxWidgetZone { get; set; }

        public bool EnableNewRibbon { get; set; }

        public bool EnableDiscountRibbon { get; set; }

        public bool EnableBestSellerRibbon { get; set; }

        public bool BestSellStoreWise { get; set; }

        public List<int> BestSellPaymentStatusIds { get; set; }

        public List<int> BestSellOrderStatusIds { get; set; }

        public List<int> BestSellShippingStatusIds { get; set; }

        public int SoldInDays { get; set; }

        public decimal MinimumAmountSold { get; set; }

        public int MinimumQuantitySold { get; set; }
    }
}
