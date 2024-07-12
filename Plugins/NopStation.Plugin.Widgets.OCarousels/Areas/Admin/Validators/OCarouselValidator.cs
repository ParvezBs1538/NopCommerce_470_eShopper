using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Validators
{
    public class OCarouselValidator : BaseNopValidator<OCarouselModel>
    {
        public OCarouselValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.Fields.Name.Required").Result);
            RuleFor(x => x.Title).NotEmpty().When(x => x.DisplayTitle).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.Fields.Title.Required").Result);

            RuleFor(x => x.NumberOfItemsToShow)
                .GreaterThan(0)
                .When(x => x.DataSourceTypeId != (int)DataSourceTypeEnum.CustomProducts)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required").Result);
        }
    }
}
