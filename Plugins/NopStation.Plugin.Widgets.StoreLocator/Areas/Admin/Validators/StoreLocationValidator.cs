using FluentValidation;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Widgets.StoreLocator.Validators
{
    public class StoreLocationValidator : BaseNopValidator<StoreLocationModel>
    {
        public StoreLocationValidator(ILocalizationService localizationService)
        {
            RuleFor(store => store.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.StoreLocations.Fields.Name.Required").Result);
            RuleFor(store => store.FullAddress)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.StoreLocations.Fields.FullAddress.Required").Result);
            RuleFor(store => store.ShortDescription)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.StoreLocations.Fields.ShortDescription.Required").Result);
        }
    }
}
