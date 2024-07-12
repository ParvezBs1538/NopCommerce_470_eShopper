using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<ProductRibbonSettings, ConfigurationModel>()
                    .ForMember(model => model.EnableBestSellerRibbon_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.EnableDiscountRibbon_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.EnableNewRibbon_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.ProductDetailsPageWidgetZone_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.BestSellOrderStatusIds_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.BestSellPaymentStatusIds_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.BestSellShippingStatusIds_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.BestSellStoreWise_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.MinimumAmountSold_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.MinimumQuantitySold_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.SoldInDays_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.AvailableOrderStatuses, options => options.Ignore())
                    .ForMember(model => model.AvailablePaymentStatuses, options => options.Ignore())
                    .ForMember(model => model.AvailableShippingStatuses, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.CurrencyCode, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, ProductRibbonSettings>();
        }
    }
}
