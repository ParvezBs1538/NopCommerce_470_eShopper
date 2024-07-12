using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.MegaMenu.Domains;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<MegaMenuSettings, ConfigurationModel>()
            .ForMember(model => model.HideManufacturers_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.EnableMegaMenu_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.MaxCategoryLevelsToShow_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.SelectedCategoryIds_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowCategoryPicture_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.SelectedManufacturerIds_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowNumberOfCategoryProductsIncludeSubcategories_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowNumberofCategoryProducts_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowManufacturerPicture_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowSubcategoryPicture_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowMainCategoryPictureRight_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.SelectedManufacturerIds, options => options.Ignore())
            .ForMember(model => model.SelectedCategoryIds, options => options.Ignore())
            .ForMember(model => model.CustomProperties, options => options.Ignore())
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, MegaMenuSettings>()
            .ForMember(entity => entity.SelectedManufacturerIds, options => options.Ignore())
            .ForMember(entity => entity.SelectedCategoryIds, options => options.Ignore());

        CreateMap<CategoryIcon, CategoryIconModel>()
            .ForMember(model => model.CategoryName, options => options.Ignore())
            .ForMember(model => model.PictureUrl, options => options.Ignore());
        CreateMap<CategoryIconModel, CategoryIcon>();
    }
}
