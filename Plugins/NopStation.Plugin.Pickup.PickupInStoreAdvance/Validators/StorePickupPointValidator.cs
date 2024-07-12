using FluentValidation;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Validators
{
    public partial class StorePickupPointValidator : BaseNopValidator<StorePickupPointModel>
    {
        public StorePickupPointValidator(ILocalizationService localizationService)
        {
            // Latitude
            RuleFor(model => model.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.InvalidRange").Result)
                .When(model => model.Latitude.HasValue);
            RuleFor(model => model.Latitude)
                .Must(latitude => latitude.HasValue)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.IsNullWhenLongitudeHasValue").Result)
                .When(model => model.Longitude.HasValue);
            RuleFor(model => model.Latitude)
                .PrecisionScale(18, 8, false)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.InvalidPrecision").Result);

            // Longitude
            RuleFor(model => model.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.InvalidRange").Result)
                .When(model => model.Longitude.HasValue);
            RuleFor(model => model.Longitude)
                .Must(longitude => longitude.HasValue)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.IsNullWhenLatitudeHasValue").Result)
                .When(model => model.Latitude.HasValue);
            RuleFor(model => model.Longitude)
                .PrecisionScale(18, 8, false)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.InvalidPrecision").Result);
        }
    }
}
