using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.Helpdesk.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Validators
{
    public class TicketValidator : BaseNopValidator<TicketModel>
    {
        public TicketValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Name.Required").Result);
            RuleFor(x => x.Body)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Body.Required").Result);
            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Subject.Required").Result);
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.Email.Required").Result);
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("NopStation.Helpdesk.Tickets.PhoneNumber.Required").Result);
        }
    }
}