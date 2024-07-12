using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipmentPickupPoint;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Validators.ShipmentPickupPoint
{
    public class ShipmentPickupPointModelValidator : BaseNopValidator<ShipmentPickupPointModel>
    {
        public ShipmentPickupPointModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .NotNull()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Name.Required"));
        }
    }
}
