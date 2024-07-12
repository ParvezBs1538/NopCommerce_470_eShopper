using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.AmazonPersonalize
{
    public class AmazonPersonalizeSettings : ISettings
    {
        public bool EnableAmazonPersonalize { get; set; }

        #region Keys

        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public int AwsRegionId { get; set; }
        public string EventTrackerId { get; set; }
        public string DataSetGroupArn { get; set; }
        public bool EnableLogging { get; set; }

        #endregion Keys

        #region RecommendedForYou

        public bool EnableRecommendedForYou { get; set; }
        public string RecommendedForYouARN { get; set; }
        public int RecommendedForYouWidgetZoneId { get; set; }
        public int RecommendedForYouNumberOfItems { get; set; }
        public bool RecommendedForYouAllowForGuestCustomer { get; set; }

        #endregion RecommendedForYou

        #region MostViewed
        public bool EnableMostViewed { get; set; }
        public string MostViewedARN { get; set; }
        public int MostViewedWidgetZoneId { get; set; }
        public int MostViewedNumberOfItems { get; set; }
        public bool MostViewedAllowForGuestCustomer { get; set; }

        #endregion MostViewed

        #region CustomersWhoViewedXAlsoViewed

        public bool EnableCustomersWhoViewedXAlsoViewed { get; set; }
        public string CustomersWhoViewedXAlsoViewedARN { get; set; }
        public int CustomersWhoViewedXAlsoViewedWidgetZoneId { get; set; }
        public int CustomersWhoViewedXAlsoViewedNumberOfItems { get; set; }
        public bool CustomersWhoViewedXAlsoViewedAllowForGuestCustomer { get; set; }

        #endregion CustomersWhoViewedXAlsoViewed

        #region BestSellers

        public bool EnableBestSellers { get; set; }
        public string BestSellersARN { get; set; }
        public int BestSellersWidgetZoneId { get; set; }
        public int BestSellersNumberOfItems { get; set; }
        public bool BestSellersAllowForGuestCustomer { get; set; }

        #endregion BestSellers

        #region FrequentlyBoughtTogether

        public bool EnableFrequentlyBoughtTogether { get; set; }
        public string FrequentlyBoughtTogetherARN { get; set; }
        public int FrequentlyBoughtTogetherWidgetZoneId { get; set; }
        public int FrequentlyBoughtTogetherNumberOfItems { get; set; }
        public bool FrequentlyBoughtTogetherAllowForGuestCustomer { get; set; }

        #endregion FrequentlyBoughtTogether
    }
}