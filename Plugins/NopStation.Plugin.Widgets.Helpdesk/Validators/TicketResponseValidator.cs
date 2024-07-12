using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.Helpdesk.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Validators
{
    public class TicketResponseValidator : BaseNopValidator<TicketResponseModel>
    {
        public TicketResponseValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Body)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("NopStation.Helpdesk.TicketResponses.Body.Required").Result);
        }
    }
}