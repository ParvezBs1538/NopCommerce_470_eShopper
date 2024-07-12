using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableAwsRegions = new List<SelectListItem>();
            AvailableWidgetZones = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableAmazonPersonalize")]
        public bool EnableAmazonPersonalize { get; set; }
        public bool EnableAmazonPersonalize_OverrideForStore { get; set; }

        #region Keys

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.AccessKey")]
        public string AccessKey { get; set; }
        public bool AccessKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.AwsRegion")]
        public int AwsRegionId { get; set; }
        public bool AwsRegionId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EventTrackerId")]
        public string EventTrackerId { get; set; }
        public bool EventTrackerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.DataSetGroupArn")]
        public string DataSetGroupArn { get; set; }
        public bool DataSetGroupArn_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableLogging")]
        public bool EnableLogging { get; set; }
        public bool EnableLogging_OverrideForStore { get; set; }


        #endregion Keys

        #region RecommendedForYou

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableRecommendedForYou")]
        public bool EnableRecommendedForYou { get; set; }
        public bool EnableRecommendedForYou_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouARN")]
        public string RecommendedForYouARN { get; set; }
        public bool RecommendedForYouARN_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouWidgetZoneId")]
        public int RecommendedForYouWidgetZoneId { get; set; }
        public bool RecommendedForYouWidgetZoneId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouNumberOfItems")]
        public int RecommendedForYouNumberOfItems { get; set; }
        public bool RecommendedForYouNumberOfItems_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouAllowForGuestCustomer")]
        public bool RecommendedForYouAllowForGuestCustomer { get; set; }
        public bool RecommendedForYouAllowForGuestCustomer_OverrideForStore { get; set; }

        #endregion RecommendedForYou

        #region MostViewed

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableMostViewed")]
        public bool EnableMostViewed { get; set; }
        public bool EnableMostViewed_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedARN")]
        public string MostViewedARN { get; set; }
        public bool MostViewedARN_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedWidgetZoneId")]
        public int MostViewedWidgetZoneId { get; set; }
        public bool MostViewedWidgetZoneId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedNumberOfItems")]
        public int MostViewedNumberOfItems { get; set; }
        public bool MostViewedNumberOfItems_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedAllowForGuestCustomer")]
        public bool MostViewedAllowForGuestCustomer { get; set; }
        public bool MostViewedAllowForGuestCustomer_OverrideForStore { get; set; }

        #endregion MostViewed

        #region CustomersWhoViewedXAlsoViewed

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableCustomersWhoViewedXAlsoViewed")]
        public bool EnableCustomersWhoViewedXAlsoViewed { get; set; }
        public bool EnableCustomersWhoViewedXAlsoViewed_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedARN")]
        public string CustomersWhoViewedXAlsoViewedARN { get; set; }
        public bool CustomersWhoViewedXAlsoViewedARN_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedWidgetZoneId")]
        public int CustomersWhoViewedXAlsoViewedWidgetZoneId { get; set; }
        public bool CustomersWhoViewedXAlsoViewedWidgetZoneId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedNumberOfItems")]
        public int CustomersWhoViewedXAlsoViewedNumberOfItems { get; set; }
        public bool CustomersWhoViewedXAlsoViewedNumberOfItems_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer")]
        public bool CustomersWhoViewedXAlsoViewedAllowForGuestCustomer { get; set; }
        public bool CustomersWhoViewedXAlsoViewedAllowForGuestCustomer_OverrideForStore { get; set; }

        #endregion CustomersWhoViewedXAlsoViewed

        #region BestSellers

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableBestSellers")]
        public bool EnableBestSellers { get; set; }
        public bool EnableBestSellers_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersARN")]
        public string BestSellersARN { get; set; }
        public bool BestSellersARN_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersWidgetZoneId")]
        public int BestSellersWidgetZoneId { get; set; }
        public bool BestSellersWidgetZoneId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersNumberOfItems")]
        public int BestSellersNumberOfItems { get; set; }
        public bool BestSellersNumberOfItems_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersAllowForGuestCustomer")]
        public bool BestSellersAllowForGuestCustomer { get; set; }
        public bool BestSellersAllowForGuestCustomer_OverrideForStore { get; set; }

        #endregion BestSellers

        #region FrequentlyBoughtTogether

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableFrequentlyBoughtTogether")]
        public bool EnableFrequentlyBoughtTogether { get; set; }
        public bool EnableFrequentlyBoughtTogether_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherARN")]
        public string FrequentlyBoughtTogetherARN { get; set; }
        public bool FrequentlyBoughtTogetherARN_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherWidgetZoneId")]
        public int FrequentlyBoughtTogetherWidgetZoneId { get; set; }
        public bool FrequentlyBoughtTogetherWidgetZoneId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherNumberOfItems")]
        public int FrequentlyBoughtTogetherNumberOfItems { get; set; }
        public bool FrequentlyBoughtTogetherNumberOfItems_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherAllowForGuestCustomer")]
        public bool FrequentlyBoughtTogetherAllowForGuestCustomer { get; set; }
        public bool FrequentlyBoughtTogetherAllowForGuestCustomer_OverrideForStore { get; set; }

        #endregion FrequentlyBoughtTogether

        public IList<SelectListItem> AvailableAwsRegions { get; set; }
        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}