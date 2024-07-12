using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Validators;

public class SmartCarouselValidator : BaseNopValidator<SmartCarouselModel>
{
    public SmartCarouselValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Fields.Name.Required").Result);
        RuleFor(x => x.Title).NotEmpty().When(x => x.DisplayTitle).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Fields.Title.Required").Result);

        RuleFor(x => x.MaxProductsToShow)
            .GreaterThan(0)
            .When(x => x.CarouselTypeId == (int)CarouselType.Product &&
                (x.ProductSourceTypeId == (int)ProductSourceType.NewProducts ||
                x.ProductSourceTypeId == (int)ProductSourceType.RecentlyViewedProducts ||
                x.ProductSourceTypeId == (int)ProductSourceType.HomePageProducts ||
                x.ProductSourceTypeId == (int)ProductSourceType.BestSellers))
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Carousels.Fields.MaxProductsToShow.Required").Result);
    }
}
