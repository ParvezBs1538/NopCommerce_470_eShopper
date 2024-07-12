using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<QuoteCartSettings, ConfigurationModel>()
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, QuoteCartSettings>();

        CreateMap<QuoteRequestMessage, QuoteRequestMessageModel>()
            .ForMember(x => x.IsWriter, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<QuoteFormModel, QuoteForm>();
        CreateMap<QuoteForm, QuoteFormModel>();


        CreateMap<QuoteRequestModel, QuoteRequest>();
        CreateMap<QuoteRequest, QuoteRequestModel>();

        CreateMap<QuoteRequestItemModel, QuoteRequestItem>();
        CreateMap<QuoteRequestItem, QuoteRequestItemModel>();

        CreateMap<PredefinedFormAttributeValue, PredefinedFormAttributeValueModel>()
            .ReverseMap();

        CreateMap<FormAttribute, FormAttributeModel>()
            .ReverseMap();

        CreateMap<FormAttributeMapping, FormAttributeMappingModel>()
            .ForMember(x => x.FormId, opt => opt.MapFrom(x => x.QuoteFormId))
            .ReverseMap();

        CreateMap<FormAttributeValueModel, FormAttributeValue>()
            .ReverseMap();
    }
}
