using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Validators
{
    public class CourierShipmentModelValidator : BaseNopValidator<CourierShipmentModel>
    {
        public CourierShipmentModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ShipmentPickupPointId).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentPickupPoint.Required"));
            RuleFor(x => x.ShipperId).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DMS.CourierShipments.Fields.Shipper.Required"));
        }
    }
}
