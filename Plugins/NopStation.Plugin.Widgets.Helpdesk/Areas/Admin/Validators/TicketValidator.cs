using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Validators
{
    public class TicketValidator : BaseNopValidator<TicketModel>
    {
        public TicketValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Fields.Name.Required").Result);
            RuleFor(x => x.Body)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Fields.Body.Required").Result);
            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Fields.Subject.Required").Result);
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Fields.Email.Required").Result);
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Tickets.Fields.PhoneNumber.Required").Result);
        }
    }
}
