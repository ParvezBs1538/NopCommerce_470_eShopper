using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.Opc.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.Opc.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<OpcSettings, ConfigurationModel>()
           .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore())
           .ForMember(model => model.BypassShoppingCartPage_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.DefaultBillingAddressCountryId_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.DefaultShippingAddressCountryId_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.EnableOnePageCheckout_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.PreselectPreviousBillingAddress_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.PreselectPreviousShippingAddress_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.PreselectShipToSameAddress_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.SaveBillingAddressOnChangeFields_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.SaveShippingAddressOnChangeFields_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.ShowDiscountBoxInCheckout_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.ShowEstimateShippingInCheckout_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.ShowGiftCardBoxInCheckout_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.ShowShoppingCartInCheckout_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.UpdatePaymentMethodsOnChangeBillingAddressFields_OverrideForStore, options => options.Ignore())
           .ForMember(model => model.UpdatePaymentMethodsOnChangeShippingAddressFields_OverrideForStore, options => options.Ignore());
        CreateMap<ConfigurationModel, OpcSettings>();
    }
}