using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Validators
{
    public class SliderValidator : BaseNopValidator<SliderModel>
    {
        public SliderValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PictureId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.Picture.Required").Result);

            RuleFor(x => x.ActiveEndDate)
                .GreaterThan(x => x.ActiveStartDate)
                .When(x => x.ActiveEndDate.HasValue && x.ActiveStartDate.HasValue)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.ActiveEndDate.GreaterThanStartDate").Result);

            RuleFor(x => x.EntityId)
                .GreaterThan(0)
                .When(x => x.SliderTypeId == (int)SliderType.Category ||
                    x.SliderTypeId == (int)SliderType.Manufacturer ||
                    x.SliderTypeId == (int)SliderType.Product ||
                    x.SliderTypeId == (int)SliderType.Vendor ||
                    x.SliderTypeId == (int)SliderType.Topic)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.WebApi.Sliders.Fields.EntityId.Required").Result);
        }
    }
}
