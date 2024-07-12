using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Validators;

public class AnnouncementItemValidator : BaseNopValidator<AnnouncementItemModel>
{
    public AnnouncementItemValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Announcement.AnnouncementItems.Fields.Title.Required").Result);
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Announcement.AnnouncementItems.Fields.Name.Required").Result);
    }
}
