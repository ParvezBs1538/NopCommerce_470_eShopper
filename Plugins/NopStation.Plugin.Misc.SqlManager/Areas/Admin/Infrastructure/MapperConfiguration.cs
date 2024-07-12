using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<SqlReport, SqlReportModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.AvailableParameters, options => options.Ignore())
                .ForMember(model => model.SelectedCustomerRoleIds, options => options.Ignore())
                .ForMember(model => model.AvailableCustomerRoles, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());
            CreateMap<SqlReportModel, SqlReport>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<SqlParameter, SqlParameterModel>()
                .ForMember(model => model.DataTypeStr, options => options.Ignore())
                .ForMember(model => model.AvailableDataTypes, options => options.Ignore())
                .ForMember(model => model.AddSqlParameterValueModel, options => options.Ignore())
                .ForMember(model => model.SqlParameterValueSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());
            CreateMap<SqlParameterModel, SqlParameter>()
                .ForMember(entity => entity.DataType, options => options.Ignore());

            CreateMap<SqlParameterValue, SqlParameterValueModel>()
                .ForMember(model => model.IsValid, options => options.Ignore())
                .ForMember(model => model.SqlParameterName, options => options.Ignore());
            CreateMap<SqlParameterValueModel, SqlParameterValue>();
        }
        public int Order => 0;
    }
}
