using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Validators;

public class MegaMenuValidator : BaseNopValidator<MegaMenuModel>
{
    public MegaMenuValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Name.Required").Result);
    }
}

public class MegaMenuItemValidator : BaseNopValidator<MegaMenuItemModel>
{
    public MegaMenuItemValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.MenuItemTypeId == (int)MenuItemType.CustomLink ||
                x.MenuItemTypeId == (int)MenuItemType.Topic ||
                x.MenuItemTypeId == (int)MenuItemType.ProductTag)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Title.Required").Result);
        RuleFor(x => x.Url)
            .NotEmpty()
            .When(x => x.MenuItemTypeId == (int)MenuItemType.CustomLink)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Url.Required").Result);
        RuleFor(x => x.RibbonText)
            .NotEmpty()
            .When(x => x.ShowRibbonText)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonText.Required").Result);
    }
}
