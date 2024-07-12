using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<QuoteCartItem, QuoteCartItemModel>();
        CreateMap<QuoteCartItemModel, QuoteCartItem>();

        CreateMap<QuoteRequestItem, QuoteRequestItemModel>();
        CreateMap<QuoteRequestItemModel, QuoteRequestItem>();

        CreateMap<QuoteRequestModel, QuoteRequest>();
        CreateMap<QuoteRequest, QuoteRequestModel>();
        CreateMap<QuoteForm, QuoteFormModel>();
        CreateMap<QuoteRequestMessage, QuoteRequestMessageModel>();
    }
}
