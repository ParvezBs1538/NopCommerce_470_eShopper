using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Validators;

public class SmartDealCarouselValidator : BaseNopValidator<SmartDealCarouselModel>
{
    public SmartDealCarouselValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Fields.Name.Required").Result);
        RuleFor(x => x.Title).NotEmpty().When(x => x.DisplayTitle).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Fields.Title.Required").Result);

        RuleFor(x => x.MaxProductsToShow)
            .GreaterThan(0)
            .When(x => x.ProductSourceTypeId == (int)ProductSourceType.Discount)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Fields.MaxProductsToShow.Required").Result);

        RuleFor(x => x.Schedule.AvaliableDateTimeToUtc)
            .NotNull()
            .When(x => x.ProductSourceTypeId == (int)ProductSourceType.CustomProducts)
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Carousels.Fields.AvaliableDateTimeToUtc.Required").Result);
    }
}
