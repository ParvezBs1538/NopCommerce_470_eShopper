using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<SmartMegaMenuSettings, ConfigurationModel>()
            .ForMember(model => model.EnableMegaMenu_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.HideDefaultMenu_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.MenuItemPictureSize_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.CustomProperties, options => options.Ignore())
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, SmartMegaMenuSettings>();

        CreateMap<MegaMenu, MegaMenuModel>()
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
        CreateMap<MegaMenuModel, MegaMenu>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

        CreateMap<MegaMenuItem, MegaMenuItemModel>()
                .ForMember(model => model.CustomProperties, options => options.Ignore());
        CreateMap<MegaMenuItemModel, MegaMenuItem>()
                .ForMember(entity => entity.CategoryId, options => options.Ignore())
                .ForMember(entity => entity.ManufacturerId, options => options.Ignore())
                .ForMember(entity => entity.VendorId, options => options.Ignore())
                .ForMember(entity => entity.ProductTagId, options => options.Ignore())
                .ForMember(entity => entity.PageTypeId, options => options.Ignore())
                .ForMember(entity => entity.TopicId, options => options.Ignore())
                .ForMember(entity => entity.MegaMenuId, options => options.Ignore())
                .ForMember(entity => entity.DisplayOrder, options => options.Ignore())
                .ForMember(entity => entity.MenuItemTypeId, options => options.Ignore());
    }
}
