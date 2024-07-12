using FluentValidation;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Validators
{
    public class DeliverySlotValidator : BaseNopValidator<DeliverySlotModel>
    {
        public DeliverySlotValidator()
        {
            RuleFor(x => x.TimeSlot).NotEmpty().WithMessage("Admin.NopStation.DeliveryScheduler.DeliverySlots.Fields.TimeSlot.Required");
        }
    }
}
