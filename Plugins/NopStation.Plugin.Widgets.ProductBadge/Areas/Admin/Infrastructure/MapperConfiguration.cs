using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<ProductBadgeSettings, ConfigurationModel>()
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, ProductBadgeSettings>();

        CreateMap<Badge, BadgeModel>()
            .ForMember(model => model.CurrencyCode, options => options.Ignore())
            .ForMember(model => model.BestSellOrderStatusIds, options => options.Ignore())
            .ForMember(model => model.BestSellPaymentStatusIds, options => options.Ignore())
            .ForMember(model => model.BestSellShippingStatusIds, options => options.Ignore())
            .ForMember(model => model.PositionTypeStr, options => options.Ignore())
            .ForMember(model => model.AvailableStores, options => options.Ignore())
            .ForMember(model => model.CustomProperties, options => options.Ignore())
            .ForMember(model => model.SelectedCustomerRoleIds, options => options.Ignore())
            .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
        CreateMap<BadgeModel, Badge>()
            .ForMember(entity => entity.BestSellOrderStatusIds, options => options.Ignore())
            .ForMember(entity => entity.BestSellPaymentStatusIds, options => options.Ignore())
            .ForMember(entity => entity.BestSellShippingStatusIds, options => options.Ignore());

        CreateMap<BadgeCategoryMapping, BadgeCategoryModel>()
            .ForMember(entity => entity.CategoryName, options => options.Ignore())
            .ForMember(entity => entity.CategoryId, options => options.Ignore());
        CreateMap<BadgeCategoryModel, BadgeCategoryMapping>();

        CreateMap<BadgeManufacturerMapping, BadgeManufacturerModel>()
            .ForMember(entity => entity.ManufacturerId, options => options.Ignore())
            .ForMember(entity => entity.ManufacturerName, options => options.Ignore());
        CreateMap<BadgeManufacturerModel, BadgeManufacturerMapping>();

        CreateMap<BadgeVendorMapping, BadgeVendorModel>()
            .ForMember(entity => entity.VendorId, options => options.Ignore())
            .ForMember(entity => entity.VendorName, options => options.Ignore());
        CreateMap<BadgeVendorModel, BadgeVendorMapping>();

        CreateMap<BadgeProductMapping, BadgeProductModel>()
            .ForMember(model => model.ProductId, options => options.Ignore())
            .ForMember(model => model.ProductName, options => options.Ignore());
        CreateMap<BadgeProductModel, BadgeProductMapping>();
    }
}