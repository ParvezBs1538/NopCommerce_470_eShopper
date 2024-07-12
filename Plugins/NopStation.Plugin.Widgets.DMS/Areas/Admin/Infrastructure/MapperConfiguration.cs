using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipmentPickupPoint;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipperDevice;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Configuration

            CreateMap<DMSSettings, ConfigurationModel>()
                .ForMember(model => model.CheckIat_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableJwtSecurity_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TokenKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TokenSecondsValid_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TokenSecret_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShipmentPageSize_OverrideForStore, options => options.Ignore())
                //.ForMember(model => model.EnableSignatureUpload_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowShippersToSelectPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PageSizeOptions_OverrideForStore, options => options.Ignore())
                //.ForMember(model => model.SignatureUploadRequired_OverrideForStore, options => options.Ignore())
                ;
            CreateMap<ConfigurationModel, DMSSettings>();

            #endregion

            #region Shipper

            CreateMap<Shipper, ShipperModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CustomerName, options => options.Ignore());
            CreateMap<ShipperModel, Shipper>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomerId, options => options.Ignore());

            #endregion

            #region Shipment

            CreateMap<CourierShipmentModel, ShipmentModel>();
            CreateMap<ShipmentModel, CourierShipmentModel>();

            #endregion

            #region ShipmentPickupPoint

            CreateMap<ShipmentPickupPoint, ShipmentPickupPointModel>()
                .ForMember(model => model.Address, options => options.Ignore());
            CreateMap<ShipmentPickupPointModel, ShipmentPickupPoint>()
                .ForMember(entity => entity.AddressId, options => options.Ignore());

            #endregion

            #region ShipperDevice


            CreateMap<ShipperDevice, DeviceModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.DeviceTypeStr, options => options.Ignore());
            CreateMap<DeviceModel, ShipperDevice>()
                .ForMember(entity => entity.DeviceType, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            #endregion

        }

        public int Order => 0;
    }
}
