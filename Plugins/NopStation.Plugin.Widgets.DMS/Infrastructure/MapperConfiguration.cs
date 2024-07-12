using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Models.Shippers;

namespace NopStation.Plugin.Widgets.DMS.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public MapperConfiguration()
        {
            CreateMap<ShipperLoginModel, LoginModel>();
            CreateMap<LoginModel, ShipperLoginModel>()
                .ForMember(model => model.LanguageNavSelector, options => options.Ignore());

            CreateMap<ProofOfDeliveryData, ProofOfDeliveryDataModel>()
                .ForMember(model => model.PODContainPhoto, options => options.Ignore())
                .ForMember(model => model.ProofOfDeliveryType, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.VerifiedOn, options => options.Ignore())
                .ForMember(model => model.PODPhotoUrl, options => options.Ignore());

            CreateMap<ProofOfDeliveryDataModel, ProofOfDeliveryData>()
                .ForMember(entity => entity.ProofOfDeliveryReferenceId, options => options.Ignore())
                .ForMember(model => model.CreatedOnUtc, options => options.Ignore())
                .ForMember(model => model.UpdatedOnUtc, options => options.Ignore())
                .ForMember(model => model.VerifiedOnUtc, options => options.Ignore())
                .ForMember(model => model.ProofOfDeliveryTypeId, options => options.Ignore());

            CreateMap<DMSShipmentNoteModel, ShipmentNote>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            CreateMap<ShipmentNote, DMSShipmentNoteModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());

            CreateMap<ShipmentPickupPoint, ShipmentPickupPointModel>()
                .ForMember(model => model.Address, options => options.Ignore());
            CreateMap<ShipmentPickupPointModel, ShipmentPickupPoint>()
                .ForMember(entity => entity.AddressId, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore());
        }

        #endregion

        #region Properties

        public int Order => 0;

        #endregion
    }
}