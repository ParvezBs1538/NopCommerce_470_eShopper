using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widget.BlogNews.Areas.Admin.Models;

namespace NopStation.Plugin.Widget.BlogNews.Areas.Admin.Validators;

public class BlogNewsValidator : BaseNopValidator<ConfigurationModel>
{
    public BlogNewsValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.WidgetZone).NotEmpty().WithMessage(localizationService.GetResourceAsync("NopStation.BlogNews.Configuration.Fields.WidgetZone.Required").Result);
    }
}
