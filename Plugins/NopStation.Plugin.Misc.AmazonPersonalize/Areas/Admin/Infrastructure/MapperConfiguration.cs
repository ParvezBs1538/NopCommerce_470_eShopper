using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Properties

        public int Order => 0;

        #endregion Properties

        #region Ctor

        public MapperConfiguration()
        {
            CreateMap<AmazonPersonalizeSettings, ConfigurationModel>()
                .ForMember(model => model.AccessKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SecretKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AwsRegionId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EventTrackerId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DataSetGroupArn_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableLogging_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableRecommendedForYou_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RecommendedForYouARN_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RecommendedForYouWidgetZoneId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RecommendedForYouNumberOfItems_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RecommendedForYouAllowForGuestCustomer_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableMostViewed_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MostViewedARN_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MostViewedWidgetZoneId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MostViewedNumberOfItems_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MostViewedAllowForGuestCustomer_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableCustomersWhoViewedXAlsoViewed_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomersWhoViewedXAlsoViewedARN_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomersWhoViewedXAlsoViewedWidgetZoneId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomersWhoViewedXAlsoViewedNumberOfItems_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableBestSellers_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BestSellersARN_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BestSellersWidgetZoneId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BestSellersNumberOfItems_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BestSellersAllowForGuestCustomer_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableFrequentlyBoughtTogether_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.FrequentlyBoughtTogetherARN_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.FrequentlyBoughtTogetherWidgetZoneId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.FrequentlyBoughtTogetherNumberOfItems_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.FrequentlyBoughtTogetherAllowForGuestCustomer_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, AmazonPersonalizeSettings>();
        }

        #endregion Ctor
    }
}