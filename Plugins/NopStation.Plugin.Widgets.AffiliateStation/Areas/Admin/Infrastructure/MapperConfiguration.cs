using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Affiliate customer

            CreateMap<AffiliateCustomer, AffiliateCustomerModel>()
                .ForMember(model => model.CustomerFullName, options => options.Ignore())
                .ForMember(model => model.ApplyStatus, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.AvailableApplyStatuses, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<AffiliateCustomerModel, AffiliateCustomer>()
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.AffiliateId, options => options.Ignore())
                .ForMember(entity => entity.CustomerId, options => options.Ignore())
                .ForMember(entity => entity.ApplyStatus, options => options.Ignore());

            #endregion 

            #region Catalog commission

            CreateMap<CatalogCommission, CatalogCommissionModel>()
                .ForMember(model => model.Name, options => options.Ignore())
                .ForMember(model => model.Commission, options => options.Ignore())
                .ForMember(model => model.ViewPath, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore());
            CreateMap<CatalogCommissionModel, CatalogCommission>();

            #endregion 

            #region Order commission

            CreateMap<OrderCommission, OrderCommissionModel>()
                .ForMember(model => model.CommissionStatus, options => options.Ignore())
                .ForMember(model => model.CommissionPaidOn, options => options.Ignore())
                .ForMember(model => model.PaymentStatus, options => options.Ignore())
                .ForMember(model => model.OrderStatusId, options => options.Ignore())
                .ForMember(model => model.OrderStatus, options => options.Ignore())
                .ForMember(model => model.PaymentStatusId, options => options.Ignore())
                .ForMember(model => model.PaymentStatus, options => options.Ignore())
                .ForMember(model => model.AffiliateId, options => options.Ignore())
                .ForMember(model => model.AffiliateName, options => options.Ignore())
                .ForMember(model => model.CustomerId, options => options.Ignore())
                .ForMember(model => model.CustomerName, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore())
                .ForMember(model => model.TotalCommissionAmountStr, options => options.Ignore())
                .ForMember(model => model.AvailableCommissionStatuses, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<OrderCommissionModel, OrderCommission>()
                .ForMember(model => model.TotalCommissionAmount, options => options.Ignore())
                .ForMember(model => model.OrderId, options => options.Ignore())
                .ForMember(model => model.CommissionPaidOn, options => options.Ignore())
                .ForMember(model => model.CommissionStatus, options => options.Ignore())
                .ForMember(model => model.AffiliateId, options => options.Ignore());

            #endregion 

            #region Settings

            CreateMap<AffiliateStationSettings, ConfigurationModel>()
                .ForMember(model => model.UsePercentage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UseDefaultCommissionIfNotSetOnCatalog_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CommissionPercentage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CommissionAmount_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AffiliatePageOrderPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore());
            CreateMap<ConfigurationModel, AffiliateStationSettings>();

            #endregion
        }

        public int Order => 0;
    }
}
